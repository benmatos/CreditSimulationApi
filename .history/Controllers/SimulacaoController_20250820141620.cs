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
        private readonly AppDbContext _context;
        private readonly IEventHubJsonWriter _eventHubWriter;

        public simulacaoController(AppDbContext context, IEventHubJsonWriter eventHubWriter)
        {
            _context = context;
            _eventHubWriter = eventHubWriter;
        }

        public class simulacaoRequest
        {
            public decimal valorDesejado { get; set; }
            public int prazo { get; set; }
        }

        public class Installment
        {
            public int Number { get; set; }
            public decimal Amortization { get; set; }
            public decimal Interest { get; set; }
            public decimal Value { get; set; }
        }

        public class simulacaoResult
        {
            public string Type { get; set; } = string.Empty;
            public List<Installment> Installments { get; set; } = new();
        }

        public class simulacaoResponse
        {
            public long simulacaoId { get; set; }
            public int ProductCode { get; set; }
            public string ProductDescription { get; set; } = string.Empty;
            public decimal MonthlyInterestRate { get; set; }
            public List<simulacaoResult> Results { get; set; } = new();
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
                        Number = i,
                        Amortization = Math.Round(amortization, 2),
                        Interest = Math.Round(interest, 2),
                        Value = Math.Round(priceInstallment, 2)
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
                        Number = i,
                        Amortization = Math.Round(sacAmortization, 2),
                        Interest = Math.Round(interest, 2),
                        Value = Math.Round(installment, 2)
                    });
                    outstanding -= sacAmortization;
                }

                // Persist simulação
                var simulacao = new Simulacao
                {
                    ProdutoId = product.Id,
                    Valor = valorDesejado,
                    Prazo = prazo,
                    DataSimulacao = DateTime.UtcNow,
                    Metodo = "API"
                };
                _simulationContext.Simulacoes.Add(simulacao);
                await _context.SaveChangesAsync();

                // envia para EventHub
                var eventData = new
                {
                    simulacaoId = simulacao.Id,
                    ProductCode = product.Id,
                    ProductDescription = product.NoProduto,
                    MonthlyInterestRate = rate,
                    valorDesejado = valorDesejado,
                    prazo = prazo,
                    Date = simulacao.DataSimulacao
                };
                await _eventHubWriter.WriteJsonAsync(eventData);

                var response = new simulacaoResponse
                {
                    simulacaoId = simulacao.Id,
                    ProductCode = product.Id,
                    ProductDescription = product.NoProduto,
                    MonthlyInterestRate = rate,
                    Results = new List<simulacaoResult>
                    {
                        new simulacaoResult{ Type = "PRICE", Installments = priceInstallments },
                        new simulacaoResult{ Type = "SAC", Installments = sacInstallments }
                    }
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                // Log exception as needed
                return StatusCode(500, $"Internal error: {ex.Message}");
            }
        }

        [HttpGet("list_simulactions")]
        public async Task<ActionResult<IEnumerable<Simulacao>>> Getsimulacaos()
        {
            return await _simulationContext.Simulacoes
                .Include(s => s.Produto)
                .OrderByDescending(s => s.DataSimulacao)
                .ToListAsync();
        }

        [HttpGet("volume_simulactions")]
        public async Task<ActionResult<IEnumerable<object>>> GetSimulatedVolumes()
        {
            var grouped = await _simulationContext.Simulacoes
                .GroupBy(s => new { s.Produto.NoProduto, Day = s.DataSimulacao.Date })
                .Select(g => new
                {
                    Product = g.Key.NoProduto,
                    Day = g.Key.Day,
                    Total = g.Count(),
                    AverageValue = g.Average(s => s.Valor),
                    MinValue = g.Min(s => s.Valor),
                    MaxValue = g.Max(s => s.Valor)
                })
                .ToListAsync();

            return grouped;
        }

        [HttpGet("telemetry")]
        public async Task<ActionResult<IEnumerable<object>>> GetTelemetry()
        {
            var telemetry = await _simulationContext.Simulacoes
                .GroupBy(s => new { s.Metodo, Day = s.DataSimulacao.Date })
                .Select(g => new
                {
                    Method = g.Key.Metodo,
                    Day = g.Key.Day,
                    Total = g.Count(),
                    AverageValorDesejado = g.Average(s => s.Valor),
                    AveragePrazo = g.Average(s => s.Prazo)
                })
                .ToListAsync();

            return telemetry;
        }
    }
}