using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CreditSimulationApi.Data;
using CreditSimulationApi.Models;

namespace CreditSimulationApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SimulacaoController : ControllerBase
    {
        private readonly AppDbContext _context;

        public SimulacaoController(AppDbContext context)
        {
            _context = context;
        }

        public class SimulacaoRequest
        {
            public decimal valorDesejado { get; set; }
            public int prazo { get; set; }
        }

        public class Parcela
        {
            public int numero { get; set; }
            public decimal valorAmortizacao { get; set; }
            public decimal valorJuros { get; set; }
            public decimal valorPrestacao { get; set; }

        }

        public class ResultadoSimulacao
        {
            public string tipo { get; set; }
            public List<Parcela> parcelas { get; set; }
        }

        public class SimulacaoResponse
        {
            public long idSimulacao { get; set; }
            public int codigoProduto { get; set; }
            public string descricaoProduto { get; set; }
            public double taxaJuros { get; set; }
            public List<ResultadoSimulacao> resultadoSimulacao { get; set; }
        }

        [HttpPost("simular")]
        public async Task<ActionResult<SimulacaoResponse>> SimularEmprestimo([FromBody] SimulacaoRequest request)
        {
            var produto = await _productContext.Produtos
                .FirstOrDefaultAsync(p =>
                    request.valorDesejado >= p.VrMinimo &&
                    (p.VrMaximo == null || request.valorDesejado <= p.VrMaximo) &&
                    request.prazo >= p.NuMinimoMeses &&
                    (p.NuMaximoMeses == null || request.prazo <= p.NuMaximoMeses));

            if (produto == null)
                return BadRequest("Nenhum produto disponível para o valor e prazo informados.");

            decimal taxa = produto.PcTaxaJuros;
            int prazo = request.prazo;
            decimal valor = request.valorDesejado;

            decimal parcelaPrice = valor * (taxa * (decimal)Math.Pow((double)(1 + taxa), prazo)) /
                                   ((decimal)Math.Pow((double)(1 + taxa), prazo) - 1);
            var simulacaoPrice = Enumerable.Range(1, prazo)
                .Select(m => new Parcela { Mes = m, Valor = Math.Round(parcelaPrice, 2) })
                .ToList();

            decimal amortizacao = valor / prazo;
            var simulacaoSAC = new List<Parcela>();
            for (int i = 0; i < prazo; i++)
            {
                decimal saldoDevedor = valor - amortizacao * i;
                decimal juros = saldoDevedor * taxa;
                decimal parcela = amortizacao + juros;
                simulacaoSAC.Add(new Parcela { Mes = i + 1, Valor = Math.Round(parcela, 2) });
            }

            return new SimulacaoResponse
            {
                Produto = produto.NoProduto,
                TaxaJurosMensal = taxa,
                SimulacaoPrice = simulacaoPrice,
                SimulacaoSAC = simulacaoSAC
            };
        }

        // Endpoint 1: Listar todas as simulações
        [HttpGet("listar_simulacoes")]
        public async Task<ActionResult<IEnumerable<Simulacao>>> GetSimulacoes()
        {
            return await _simulationContext.Simulacoes.Include(s => s.Produto).OrderByDescending(s => s.DataSimulacao).ToListAsync();
        }

        // Endpoint 2: Valores simulados por produto e por dia
        [HttpGet("volume_simulado")]
        public async Task<ActionResult<IEnumerable<object>>> GetSimulacoesAgrupadas()
        {
            var agrupado = await _simulationContext.Simulacoes
                .GroupBy(s => new { s.Produto.NoProduto, Dia = s.DataSimulacao.Date })
                .Select(g => new {
                    Produto = g.Key.NoProduto,
                    Dia = g.Key.Dia,
                    Total = g.Count(),
                    ValorMedio = g.Average(s => s.Valor),
                    ValorMinimo = g.Min(s => s.Valor),
                    ValorMaximo = g.Max(s => s.Valor)
                })
                .ToListAsync();

            return agrupado;
        }

        // Endpoint 3: Dados de telemetria por método e por dia
        [HttpGet("telemetria")]
        public async Task<ActionResult<IEnumerable<object>>> GetTelemetria()
        {
            var telemetria = await _simulationContext.Simulacoes
                .GroupBy(s => new { s.Metodo, Dia = s.DataSimulacao.Date })
                .Select(g => new {
                    Metodo = g.Key.Metodo,
                    Dia = g.Key.Dia,
                    Total = g.Count(),
                    ValorMedio = g.Average(s => s.Valor),
                    PrazoMedio = g.Average(s => s.Prazo)
                })
                .ToListAsync();

            return telemetria;
        }
    }
}
