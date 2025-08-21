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

        public async Task<Simulacao> PersistirSimulacaoAsync(Produto product, decimal valorDesejado, int prazo, long tempoExecucao, bool sucesso, decimal valorTotalParcelas)
        {
            try
            {
                var simulacao = new Simulacao
                {
                    idProduto = product.CoProduto,
                    valorDesejado = valorDesejado,
                    prazo = prazo,
                    TaxaJuros = product.PcTaxaJuros,
                    DataSimulacao = DateTime.UtcNow,
                    TempoExecucaoMs = tempoExecucao,
                    Sucesso = sucesso,
                    noProduto = product.NoProduto,
                    valorTotalParcelas = valorTotalParcelas
                };
                _simulationContext.Simulacoes.Add(simulacao);
                await _simulationContext.SaveChangesAsync();
                return simulacao;
            }
            catch (Exception ex)
            {
                // Log detalhado
                throw new Exception("Erro ao persistir simulação: " + ex.Message, ex);
            }
        }

        public Task<Simulacao> PersistirSimulacaoAsync(Produto product, decimal valorDesejado, int prazo, long tempoExecucao, bool sucesso)
        {
            throw new NotImplementedException();
        }
    }
}