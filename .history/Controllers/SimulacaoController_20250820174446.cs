using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CreditsimulacaoApi.Data;
using CreditsimulacaoApi.Models;
using CreditsimulacaoApi.Services;

namespace CreditsimulacaoApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class simulacaoController : ControllerBase
    {
        private readonly ProductDbContext _productContext;
        private readonly SimulationDbContext _simulationContext;
        private readonly IEventHubJsonWriter _eventHubWriter;


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

        public class Installment
        {
            public int numero { get; set; }
            public decimal valorAmortizacao { get; set; }
            public decimal valorJuros { get; set; }
            public decimal valorPrestacao { get; set; }
        }

        public class simulacaoResult
        {
            public string tipo { get; set; } = string.Empty;
            public List<Installment> parcelas { get; set; } = new();
        }

        public class simulacaoResponse
        {
            public long idSimulacao { get; set; }
            public int codigoProduto { get; set; }
            public string descricaoProduto { get; set; } = string.Empty;
            public decimal taxaJuros { get; set; }
            public List<simulacaoResult> resultadoSimulacao { get; set; } = new();
        }

        [HttpPost("simulate")]
        public async Task<ActionResult<simulacaoResponse>> SimulateLoan([FromBody] simulacaoRequest request)
        {
            try
            {
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
                decimal priceInstallment = valorDesejado * (rate * (decimal)Math.Pow((double)(1 + rate), prazo)) /
                                           ((decimal)Math.Pow((double)(1 + rate), prazo) - 1);

                var priceInstallments = new List<Installment>();
                decimal outstanding = valorDesejado;
                for (int i = 1; i <= prazo; i++)
                {
                    decimal interest = outstanding * rate;
                    decimal amortization = priceInstallment - interest;
                    priceInstallments.Add(new Installment
                    {
                        numero = i,
                        valorAmortizacao = Math.Round(amortization, 2),
                        valorJuros = Math.Round(interest, 2),
                        valorPrestacao = Math.Round(priceInstallment, 2)
                    });
                    outstanding -= amortization;
                }

                // SAC System
                decimal sacAmortization = valorDesejado / prazo;
                outstanding = valorDesejado;
                var sacInstallments = new List<Installment>();
                for (int i = 1; i <= prazo; i++)
                {
                    decimal interest = outstanding * rate;
                    decimal installment = sacAmortization + interest;
                    sacInstallments.Add(new Installment
                    {
                        numero = i,
                        valorAmortizacao = Math.Round(sacAmortization, 2),
                        valorJuros = Math.Round(interest, 2),
                        valorPrestacao = Math.Round(installment, 2)
                    });
                    outstanding -= sacAmortization;
                }

                /* Persist simulação
                var simulacao = new Simulacao
                {
                    idProduto = product.CoProduto,
                    valorDesejado = valorDesejado,
                    prazo = prazo,
                    DataSimulacao = DateTime.UtcNow,
                };
                _simulationContext.Simulacoes.Add(simulacao);
                await _context.SaveChangesAsync();*/
             

                var response = new simulacaoResponse
                {
                    idSimulacao = 1,//simulacao.idSimulacao,
                    codigoProduto = product.CoProduto,
                    descricaoProduto = product.NoProduto,
                    taxaJuros = product.PcTaxaJuros,
                    resultadoSimulacao = new List<simulacaoResult>
                    {
                        new simulacaoResult{ tipo = "PRICE", parcelas = priceInstallments },
                        new simulacaoResult{ tipo = "SAC", parcelas = sacInstallments }
                    }
                };

                // envia para EventHub
                await _eventHubWriter.WriteJsonAsync(response);

                return Ok(response);
            }
            catch (Exception ex)
            {
                // Log exception as needed
                return StatusCode(500, $"Internal error: {ex.Message}");
            }
        }

        [HttpGet("list_simulactions")]
        public async Task<ActionResult<IEnumerable<Simulacao>>> Getsimulacoes()
        {
            return await _simulationContext.Simulacoes
                .Include(s => s.idProduto)
                .OrderByDescending(s => s.DataSimulacao)
                .ToListAsync();
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
        public async Task<ActionResult<IEnumerable<object>>> GetTelemetry()
        {
            var telemetry = await _simulationContext.Simulacoes
                .GroupBy(s => new { Day = s.DataSimulacao.Date })
                .Select(g => new
                {
                    Method = "Simulacao",
                    Day = g.Key.Day,
                    Total = g.Count(),
                    AverageValorDesejado = g.Average(s => s.valorDesejado),
                    AveragePrazo = g.Average(s => s.prazo)
                })
                .ToListAsync();

            return telemetry;
        }
    }
}