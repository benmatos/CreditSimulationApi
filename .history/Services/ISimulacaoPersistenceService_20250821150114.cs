using CreditsimulacaoApi.Models;
using System.Threading.Tasks;

namespace CreditsimulacaoApi.Services
{
    public interface ISimulacaoPersistenceService
    {
        Task<Simulacao> PersistirSimulacaoAsync(Produto product, decimal valorDesejado, int prazo, long tempoExecucao, bool sucesso);
    }
}