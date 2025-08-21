using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CreditSimulationApi.Migrations.SimulationDb
{
    /// <inheritdoc />
    public partial class atualizaMetrica : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "Sucesso",
                table: "SIMULACAO",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<decimal>(
                name: "TaxaJuros",
                table: "SIMULACAO",
                type: "TEXT",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<long>(
                name: "TempoExecucaoMs",
                table: "SIMULACAO",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<decimal>(
                name: "valorTotalParcelas",
                table: "SIMULACAO",
                type: "TEXT",
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Sucesso",
                table: "SIMULACAO");

            migrationBuilder.DropColumn(
                name: "TaxaJuros",
                table: "SIMULACAO");

            migrationBuilder.DropColumn(
                name: "TempoExecucaoMs",
                table: "SIMULACAO");

            migrationBuilder.DropColumn(
                name: "valorTotalParcelas",
                table: "SIMULACAO");
        }
    }
}
