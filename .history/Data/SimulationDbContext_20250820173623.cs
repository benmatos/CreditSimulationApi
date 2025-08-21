using Microsoft.EntityFrameworkCore;
using CreditsimulacaoApi.Models;

namespace CreditsimulacaoApi.Data
{
    public class SimulationDbContext : DbContext
    {
        public SimulationDbContext(DbContextOptions<SimulationDbContext> options)
            : base(options) { }

        public DbSet<Simulacao> Simulacoes { get; set; }
    }
}