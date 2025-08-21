using CreditsimulacaoApi.Data;
using CreditsimulacaoApi.Models;
using System;
using System.Threading.Tasks;

namespace CreditsimulacaoApi.Services
{
    public class SimulacaoPersistenceService : ISimulacaoPersistenceService
    {
        private readonly SimulationDbContext _simulationContext;

        public SimulacaoPersistenceService(SimulationDbContext simulationContext)
        {
            _simulationContext = simulationContext;
        }

        public async Task<Simulacao> PersistirSimulacaoAsync(Produto product, decimal valorDesejado, int prazo, long tempoExecucao, bool sucesso)
        {
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
            return simulacao;
        }
    }
}