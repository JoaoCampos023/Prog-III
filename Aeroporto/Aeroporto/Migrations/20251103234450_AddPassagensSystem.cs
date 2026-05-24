using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SistemaAereo.Migrations
{
    /// <inheritdoc />
    public partial class AddPassagensSystem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Passagens",
                columns: table => new
                {
                    PassagemId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    VooId = table.Column<int>(type: "int", nullable: false),
                    ClienteId = table.Column<int>(type: "int", nullable: false),
                    PoltronaId = table.Column<int>(type: "int", nullable: false),
                    NumeroBilhete = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    DataEmissao = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Preco = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Classe = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Passagens", x => x.PassagemId);
                    table.ForeignKey(
                        name: "FK_Passagens_ClientesPreferenciais_ClienteId",
                        column: x => x.ClienteId,
                        principalTable: "ClientesPreferenciais",
                        principalColumn: "ClienteId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Passagens_Poltronas_PoltronaId",
                        column: x => x.PoltronaId,
                        principalTable: "Poltronas",
                        principalColumn: "PoltronaId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Passagens_Voos_VooId",
                        column: x => x.VooId,
                        principalTable: "Voos",
                        principalColumn: "VooId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Passagens_ClienteId",
                table: "Passagens",
                column: "ClienteId");

            migrationBuilder.CreateIndex(
                name: "IX_Passagens_NumeroBilhete",
                table: "Passagens",
                column: "NumeroBilhete",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Passagens_PoltronaId",
                table: "Passagens",
                column: "PoltronaId");

            migrationBuilder.CreateIndex(
                name: "IX_Passagens_VooId",
                table: "Passagens",
                column: "VooId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Passagens");
        }
    }
}
