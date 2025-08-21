using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CreditSimulationApi.Migrations.SimulationDb
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SIMULACAO",
                columns: table => new
                {
                    idSimulacao = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    valorDesejado = table.Column<decimal>(type: "TEXT", nullable: false),
                    idProduto = table.Column<int>(type: "INTEGER", nullable: false),
                    noProduto = table.Column<int>(type: "INTEGER", nullable: false),
                    prazo = table.Column<int>(type: "INTEGER", nullable: false),
                    DataSimulacao = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SIMULACAO", x => x.idSimulacao);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SIMULACAO");
        }
    }
}
