using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CreditsimulacaoApi.Data;
using CreditsimulacaoApi.Models;
using CreditsimulacaoApi.Services;
using Prometheus;
using System.Diagnostics;

namespace CreditsimulacaoApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class simulacaoController : ControllerBase
    {
        private readonly ProductDbContext _productContext;
        private readonly SimulationDbContext _simulationContext;
        private readonly IEventHubJsonWriter _eventHubWriter;

        private static readonly Counter SimulationRequests = Metrics
        .CreateCounter("simulation_requests_total", "Número total de requisições ao endpoint de simulação");

        private static readonly Histogram SimulationDuration = Metrics
        .CreateHistogram("simulation_duration_seconds", "Duração das simulações em segundos");

        public simulacaoController(ProductDbContext productContext, SimulationDbContext simulationContext, IEventHubJsonWriter eventHubWriter)
        {
            _productContext = productContext;
            _simulationContext = simulationContext;
            _eventHubWriter = eventHubWriter;
        }
        public class simulacaoRequest
        {
            public decimal valorDesejado { get; set; }
            public int prazo { get; set; }
        }

        public class Parcelas
        {
            public int numero { get; set; }
            public decimal valorAmortizacao { get; set; }
            public decimal valorJuros { get; set; }
            public decimal valorPrestacao { get; set; }
        }

        public class simulacaoResult
        {
            public string tipo { get; set; } = string.Empty;
            public List<Parcelas> parcelas { get; set; } = new();
        }

        public class simulacaoResponse
        {
            public long idSimulacao { get; set; }
            public int codigoProduto { get; set; }
            public string descricaoProduto { get; set; } = string.Empty;
            public decimal taxaJuros { get; set; }
            public List<simulacaoResult> resultadoSimulacao { get; set; } = new();
        }

        public class SimulationMetrics
        {
            public int TotalRequests { get; set; } = 0;
            public int SuccessResponses { get; set; } = 0;
            public List<long> DurationsMs { get; set; } = new();
        }

        internal static class MetricsStore
        {
            public static SimulationMetrics Simulation { get; set; } = new SimulationMetrics();
        }

        [HttpPost("simulate")]
        public async Task<ActionResult<simulacaoResponse>> SimulateLoan([FromBody] simulacaoRequest request)
        {
            try
            {
                
                var stopwatch = Stopwatch.StartNew();
                
                var product = await _productContext.Produtos
                    .FirstOrDefaultAsync(p =>
                        request.valorDesejado >= p.VrMinimo &&
                        (p.VrMaximo == null || request.valorDesejado <= p.VrMaximo) &&
                        request.prazo >= p.NuMinimoMeses &&
                        (p.NuMaximoMeses == null || request.prazo <= p.NuMaximoMeses));

                if (product == null)
                    return BadRequest("No product available for the informed valorDesejado and prazo.");

                decimal rate = product.PcTaxaJuros;
                int prazo = request.prazo;
                decimal valorDesejado = request.valorDesejado;

                // Price System
                decimal priceParcelas = valorDesejado * (rate * (decimal)Math.Pow((double)(1 + rate), prazo)) /
                                           ((decimal)Math.Pow((double)(1 + rate), prazo) - 1);

                var priceParcelass = new List<Parcelas>();
                decimal outstanding = valorDesejado;
                for (int i = 1; i <= prazo; i++)
                {
                    decimal interest = outstanding * rate;
                    decimal amortization = priceParcelas - interest;
                    priceParcelass.Add(new Parcelas
                    {
                        numero = i,
                        valorAmortizacao = Math.Round(amortization, 2),
                        valorJuros = Math.Round(interest, 2),
                        valorPrestacao = Math.Round(priceParcelas, 2)
                    });
                    outstanding -= amortization;
                }

                // SAC System
                decimal sacAmortization = valorDesejado / prazo;
                outstanding = valorDesejado;
                var sacParcelass = new List<Parcelas>();
                for (int i = 1; i <= prazo; i++)
                {
                    decimal interest = outstanding * rate;
                    decimal Parcelas = sacAmortization + interest;
                    sacParcelass.Add(new Parcelas
                    {
                        numero = i,
                        valorAmortizacao = Math.Round(sacAmortization, 2),
                        valorJuros = Math.Round(interest, 2),
                        valorPrestacao = Math.Round(Parcelas, 2)
                    });
                    outstanding -= sacAmortization;
                }

                stopwatch.Stop();
                var tempoExecucao = stopwatch.ElapsedMilliseconds;
                var sucesso = Response.StatusCode == 200;

                //Persist simulação
                var simulacao = new Simulacao
                {
                    idProduto = product.CoProduto,
                    valorDesejado = valorDesejado,
                    prazo = prazo,
                    TaxaJuros = product.PcTaxaJuros,
                    DataSimulacao = DateTime.UtcNow,
                    TempoExecucaoMs = tempoExecucao,
                    Sucesso = sucesso
                };
                _simulationContext.Simulacoes.Add(simulacao);
                await _simulationContext.SaveChangesAsync();
             

                var response = new simulacaoResponse
                {
                    idSimulacao = simulacao.idSimulacao,
                    codigoProduto = product.CoProduto,
                    descricaoProduto = product.NoProduto,
                    taxaJuros = product.PcTaxaJuros,
                    resultadoSimulacao = new List<simulacaoResult>
                    {
                        new simulacaoResult{ tipo = "PRICE", parcelas = priceParcelass },
                        new simulacaoResult{ tipo = "SAC", parcelas = sacParcelass }
                    }
                };

                // envia para EventHub
                await _eventHubWriter.WriteJsonAsync(response);

                MetricsStore.Simulation.TotalRequests++;
                MetricsStore.Simulation.DurationsMs.Add(tempoExecucao);
                if (sucesso) MetricsStore.Simulation.SuccessResponses++;

                return Ok(response);
            }
            catch (Exception ex)
            {
                // Log exception as needed
                return StatusCode(500, $"Internal error: {ex.Message}");
            }
        }

        [HttpGet("list_simulactions")]
        public async Task<ActionResult<object>> Getsimulacoes([FromQuery] int pagina = 1, [FromQuery] int qtdRegistrosPagina = 200)
        {
            if (pagina < 1) pagina = 1;
                if (qtdRegistrosPagina < 1) qtdRegistrosPagina = 200;

                var query = _simulationContext.Simulacoes
                    .OrderByDescending(s => s.DataSimulacao);

                var qtdRegistros = await query.CountAsync();

                var registros = await query
                    .Skip((pagina - 1) * qtdRegistrosPagina)
                    .Take(qtdRegistrosPagina)
                    .Select(s => new
                    {
                        idSimulacao = s.idSimulacao,
                        valorDesejado = s.valorDesejado,
                        prazo = s.prazo,
                        valorTotalParcelas = s.valorDesejado + (s.TaxaJuros * s.valorDesejado * s.prazo) // ajuste conforme sua regra de negócio
                    })
                    .ToListAsync();

                var resultado = new
                {
                    pagina,
                    qtdRegistros,
                    qtdRegistrosPagina,
                    registros
                };

                return Ok(resultado);
        }

        [HttpGet("volume_simulactions")]
        public async Task<ActionResult<IEnumerable<object>>> GetSimulatedVolumes()
        {
            var grouped = await _simulationContext.Simulacoes
                .GroupBy(s => new { s.noProduto, Day = s.DataSimulacao.Date })
                .Select(g => new
                {
                    Product = g.Key.noProduto,
                    Day = g.Key.Day,
                    Total = g.Count(),
                    AverageValue = g.Average(s => s.valorDesejado),
                    MinValue = g.Min(s => s.valorDesejado),
                    MaxValue = g.Max(s => s.valorDesejado)
                })
                .ToListAsync();

            return grouped;
        }

       
        [HttpGet("telemetry")]
        public IActionResult GetTelemetry()
        {
            var metrics = MetricsStore.Simulation;

            var qtdRequisicoes = metrics.TotalRequests;
            var tempoMedio = (int)(metrics.DurationsMs.Count > 0 ? metrics.DurationsMs.Average() : 0);
            var tempoMinimo = metrics.DurationsMs.Count > 0 ? metrics.DurationsMs.Min() : 0;
            var tempoMaximo = metrics.DurationsMs.Count > 0 ? metrics.DurationsMs.Max() : 0;
            var percentualSucesso = qtdRequisicoes > 0
                ? Math.Round((double)metrics.SuccessResponses / qtdRequisicoes, 2)
                : 0;

            var resposta = new
            {
                dataReferencia = DateTime.UtcNow.ToString("yyyy-MM-dd"),
                listaEndpoints = new[]
                {
                    new
                    {
                        nomeApi = "Simulacao",
                        qtdRequisicoes,
                        tempoMedio,
                        tempoMinimo,
                        tempoMaximo,
                        percentualSucesso
                    }
                }
            };

            return Ok(resposta);
        }
    }
}
