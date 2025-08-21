using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CreditSimulationApi.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PRODUTO",
                columns: table => new
                {
                    CO_PRODUTO = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    NO_PRODUTO = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    PC_TAXA_JUROS = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    NU_MINIMO_MESES = table.Column<short>(type: "smallint", nullable: false),
                    NU_MAXIMO_MESES = table.Column<short>(type: "smallint", nullable: true),
                    VR_MINIMO = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    VR_MAXIMO = table.Column<decimal>(type: "decimal(18,2)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PRODUTO", x => x.CO_PRODUTO);
                });

            migrationBuilder.CreateTable(
                name: "SIMULACAO",
                columns: table => new
                {
                    idSimulacao = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    valorDesejado = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    idProduto = table.Column<int>(type: "int", nullable: false),
                    noProduto = table.Column<int>(type: "int", nullable: false),
                    prazo = table.Column<int>(type: "int", nullable: false),
                    DataSimulacao = table.Column<DateTime>(type: "datetime2", nullable: false)
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
                name: "PRODUTO");

            migrationBuilder.DropTable(
                name: "SIMULACAO");
        }
    }
}
