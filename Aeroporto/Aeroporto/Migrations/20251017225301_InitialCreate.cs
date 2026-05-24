using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SistemaAereo.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Aeronaves",
                columns: table => new
                {
                    AeronaveId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TipoAeronave = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    NumeroPoltronas = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Aeronaves", x => x.AeronaveId);
                });

            migrationBuilder.CreateTable(
                name: "Aeroportos",
                columns: table => new
                {
                    AeroportoId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nome = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    CodigoIATA = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false),
                    Cidade = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Pais = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Aeroportos", x => x.AeroportoId);
                });

            migrationBuilder.CreateTable(
                name: "ClientesPreferenciais",
                columns: table => new
                {
                    ClienteId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nome = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Telefone = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    CPF = table.Column<string>(type: "nvarchar(14)", maxLength: 14, nullable: false),
                    DataNascimento = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Endereco = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Cidade = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Estado = table.Column<string>(type: "nvarchar(2)", maxLength: 2, nullable: false),
                    CEP = table.Column<string>(type: "nvarchar(9)", maxLength: 9, nullable: false),
                    DataCadastro = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Ativo = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClientesPreferenciais", x => x.ClienteId);
                });

            migrationBuilder.CreateTable(
                name: "Voos",
                columns: table => new
                {
                    VooId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    NumeroVoo = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    AeroportoOrigemId = table.Column<int>(type: "int", nullable: false),
                    AeroportoDestinoId = table.Column<int>(type: "int", nullable: false),
                    AeronaveId = table.Column<int>(type: "int", nullable: false),
                    HorarioSaida = table.Column<DateTime>(type: "datetime2", nullable: false),
                    HorarioChegadaPrevisto = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Voos", x => x.VooId);
                    table.ForeignKey(
                        name: "FK_Voos_Aeronaves_AeronaveId",
                        column: x => x.AeronaveId,
                        principalTable: "Aeronaves",
                        principalColumn: "AeronaveId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Voos_Aeroportos_AeroportoDestinoId",
                        column: x => x.AeroportoDestinoId,
                        principalTable: "Aeroportos",
                        principalColumn: "AeroportoId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Voos_Aeroportos_AeroportoOrigemId",
                        column: x => x.AeroportoOrigemId,
                        principalTable: "Aeroportos",
                        principalColumn: "AeroportoId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Escalas",
                columns: table => new
                {
                    EscalaId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    VooId = table.Column<int>(type: "int", nullable: false),
                    AeroportoId = table.Column<int>(type: "int", nullable: false),
                    Ordem = table.Column<int>(type: "int", nullable: false),
                    HorarioSaida = table.Column<DateTime>(type: "datetime2", nullable: false),
                    HorarioChegada = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Escalas", x => x.EscalaId);
                    table.ForeignKey(
                        name: "FK_Escalas_Aeroportos_AeroportoId",
                        column: x => x.AeroportoId,
                        principalTable: "Aeroportos",
                        principalColumn: "AeroportoId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Escalas_Voos_VooId",
                        column: x => x.VooId,
                        principalTable: "Voos",
                        principalColumn: "VooId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Poltronas",
                columns: table => new
                {
                    PoltronaId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    VooId = table.Column<int>(type: "int", nullable: false),
                    NumeroPoltrona = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    Disponivel = table.Column<bool>(type: "bit", nullable: false),
                    Localizacao = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Tipo = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Poltronas", x => x.PoltronaId);
                    table.ForeignKey(
                        name: "FK_Poltronas_Voos_VooId",
                        column: x => x.VooId,
                        principalTable: "Voos",
                        principalColumn: "VooId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Aeroportos_CodigoIATA",
                table: "Aeroportos",
                column: "CodigoIATA",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ClientesPreferenciais_CPF",
                table: "ClientesPreferenciais",
                column: "CPF",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ClientesPreferenciais_Email",
                table: "ClientesPreferenciais",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Escalas_AeroportoId",
                table: "Escalas",
                column: "AeroportoId");

            migrationBuilder.CreateIndex(
                name: "IX_Escalas_VooId",
                table: "Escalas",
                column: "VooId");

            migrationBuilder.CreateIndex(
                name: "IX_Poltronas_VooId",
                table: "Poltronas",
                column: "VooId");

            migrationBuilder.CreateIndex(
                name: "IX_Voos_AeronaveId",
                table: "Voos",
                column: "AeronaveId");

            migrationBuilder.CreateIndex(
                name: "IX_Voos_AeroportoDestinoId",
                table: "Voos",
                column: "AeroportoDestinoId");

            migrationBuilder.CreateIndex(
                name: "IX_Voos_AeroportoOrigemId",
                table: "Voos",
                column: "AeroportoOrigemId");

            migrationBuilder.CreateIndex(
                name: "IX_Voos_NumeroVoo",
                table: "Voos",
                column: "NumeroVoo",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ClientesPreferenciais");

            migrationBuilder.DropTable(
                name: "Escalas");

            migrationBuilder.DropTable(
                name: "Poltronas");

            migrationBuilder.DropTable(
                name: "Voos");

            migrationBuilder.DropTable(
                name: "Aeronaves");

            migrationBuilder.DropTable(
                name: "Aeroportos");
        }
    }
}
