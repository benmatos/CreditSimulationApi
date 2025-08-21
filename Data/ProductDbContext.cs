using Microsoft.EntityFrameworkCore;
using CreditsimulacaoApi.Models;

namespace CreditsimulacaoApi.Data
{
    public class ProductDbContext : DbContext
    {
        public ProductDbContext(DbContextOptions<ProductDbContext> options)
            : base(options) { }

        public DbSet<Produto> Produtos { get; set; }
    }
}