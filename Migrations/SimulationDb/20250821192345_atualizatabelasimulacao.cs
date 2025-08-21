using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CreditSimulationApi.Migrations.SimulationDb
{
    /// <inheritdoc />
    public partial class atualizatabelasimulacao : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "noProduto",
                table: "SIMULACAO",
                type: "TEXT",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "INTEGER");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "noProduto",
                table: "SIMULACAO",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "TEXT");
        }
    }
}
