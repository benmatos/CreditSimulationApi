using CreditsimulacaoApi.Data;
using CreditsimulacaoApi.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddScoped<IEventHubJsonWriter, EventHubJsonWriter>();
builder.Services.AddEndpointsApiExplorer();

// SQL Server para produtos
builder.Services.AddDbContext<ProductDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// SQLite para simulações
builder.Services.AddDbContext<SimulationDbContext>(options =>
    options.UseSqlite("Data Source=simulacoes.db"));
builder.Services.AddSwaggerGen();

    
var app = builder.Build();

// Configure the HTTP request pipeline.

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Simula��o de Empr�stimos API v1");
});

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
