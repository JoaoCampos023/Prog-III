using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaAereo.Data;
using SistemaAereo.Models;

namespace SistemaAereo.Controllers
{
    public class DebugController : Controller
    {
        private readonly AeroportoContext _context;

        public DebugController(AeroportoContext context)
        {
            _context = context;
        }

        // =============================================
        // MÉTODOS DE VISUALIZAÇÃO DE DADOS
        // =============================================

        public async Task<IActionResult> Dados()
        {
            var dados = new
            {
                Voos = await _context.Voos
                    .Include(v => v.AeroportoOrigem)
                    .Include(v => v.AeroportoDestino)
                    .Include(v => v.Aeronave)
                    .Include(v => v.Poltronas)
                    .Select(v => new
                    {
                        v.VooId,
                        v.NumeroVoo,
                        Origem = v.AeroportoOrigem.CodigoIATA,
                        Destino = v.AeroportoDestino.CodigoIATA,
                        v.HorarioSaida,
                        EhFuturo = v.HorarioSaida > DateTime.Now,
                        TotalPoltronas = v.Poltronas.Count,
                        PoltronasDisponiveis = v.Poltronas.Count(p => p.Disponivel)
                    })
                    .ToListAsync(),

                Clientes = await _context.ClientesPreferenciais
                    .Where(c => c.Ativo)
                    .Select(c => new { c.ClienteId, c.Nome })
                    .ToListAsync(),

                Aeronaves = await _context.Aeronaves
                    .Select(a => new { a.AeronaveId, a.TipoAeronave })
                    .ToListAsync(),

                Aeroportos = await _context.Aeroportos
                    .Select(a => new { a.AeroportoId, a.Nome, a.CodigoIATA })
                    .ToListAsync()
            };

            return View(dados);
        }

        public async Task<JsonResult> ApiDados()
        {
            var dados = new
            {
                Voos = await _context.Voos
                    .Include(v => v.AeroportoOrigem)
                    .Include(v => v.AeroportoDestino)
                    .Include(v => v.Poltronas)
                    .Select(v => new
                    {
                        v.VooId,
                        v.NumeroVoo,
                        Origem = v.AeroportoOrigem.CodigoIATA,
                        Destino = v.AeroportoDestino.CodigoIATA,
                        v.HorarioSaida,
                        EhFuturo = v.HorarioSaida > DateTime.Now,
                        TotalPoltronas = v.Poltronas.Count,
                        PoltronasDisponiveis = v.Poltronas.Count(p => p.Disponivel)
                    })
                    .ToListAsync(),

                ClientesAtivos = await _context.ClientesPreferenciais.CountAsync(c => c.Ativo)
            };

            return Json(dados);
        }

        // =============================================
        // MÉTODOS DE CRIAÇÃO DE DADOS TESTE
        // =============================================

        [HttpPost]
        public async Task<JsonResult> CriarDadosTeste()
        {
            try
            {
                var voosExistem = await _context.Voos.AnyAsync();
                var clientesExistem = await _context.ClientesPreferenciais.AnyAsync();

                if (voosExistem || clientesExistem)
                {
                    return Json(new
                    {
                        sucesso = false,
                        mensagem = "Já existem dados no sistema. Use os dados existentes ou limpe o banco primeiro."
                    });
                }

                var aeronave = new Aeronave
                {
                    TipoAeronave = "Boeing 737-800",
                    NumeroPoltronas = 180
                };
                _context.Aeronaves.Add(aeronave);

                var aeroportos = new[]
                {
                    new Aeroporto { Nome = "Aeroporto Internacional de São Paulo/Guarulhos", CodigoIATA = "GRU", Cidade = "São Paulo", Pais = "Brasil" },
                    new Aeroporto { Nome = "Aeroporto Santos Dumont", CodigoIATA = "SDU", Cidade = "Rio de Janeiro", Pais = "Brasil" },
                    new Aeroporto { Nome = "Aeroporto Internacional de Brasília", CodigoIATA = "BSB", Cidade = "Brasília", Pais = "Brasil" }
                };
                _context.Aeroportos.AddRange(aeroportos);

                await _context.SaveChangesAsync();

                var voos = new[]
                {
                    new Voo
                    {
                        NumeroVoo = "LA1234",
                        AeroportoOrigemId = aeroportos[0].AeroportoId,
                        AeroportoDestinoId = aeroportos[1].AeroportoId,
                        AeronaveId = aeronave.AeronaveId,
                        HorarioSaida = DateTime.Now.AddDays(1).AddHours(2),
                        HorarioChegadaPrevisto = DateTime.Now.AddDays(1).AddHours(4)
                    },
                    new Voo
                    {
                        NumeroVoo = "LA5678",
                        AeroportoOrigemId = aeroportos[0].AeroportoId,
                        AeroportoDestinoId = aeroportos[2].AeroportoId,
                        AeronaveId = aeronave.AeronaveId,
                        HorarioSaida = DateTime.Now.AddDays(2).AddHours(3),
                        HorarioChegadaPrevisto = DateTime.Now.AddDays(2).AddHours(5)
                    }
                };
                _context.Voos.AddRange(voos);

                var clientes = new[]
                {
                    new ClientePreferencial
                    {
                        Nome = "João Silva",
                        Email = "joao.silva@email.com",
                        Telefone = "(11) 99999-9999",
                        CPF = "123.456.789-00",
                        Cidade = "São Paulo",
                        Estado = "SP"
                    },
                    new ClientePreferencial
                    {
                        Nome = "Maria Santos",
                        Email = "maria.santos@email.com",
                        Telefone = "(21) 98888-8888",
                        CPF = "987.654.321-00",
                        Cidade = "Rio de Janeiro",
                        Estado = "RJ"
                    }
                };
                _context.ClientesPreferenciais.AddRange(clientes);

                await _context.SaveChangesAsync();

                foreach (var voo in voos)
                {
                    await CriarPoltronasParaVoo(voo.VooId, aeronave.NumeroPoltronas);
                }

                return Json(new
                {
                    sucesso = true,
                    mensagem = "Dados de teste criados com sucesso! Foram criados 2 voos, 2 clientes e poltronas."
                });
            }
            catch (Exception ex)
            {
                return Json(new { sucesso = false, mensagem = $"Erro ao criar dados de teste: {ex.Message}" });
            }
        }

        // =============================================
        // MÉTODOS PRIVADOS AUXILIARES
        // =============================================

        private async Task CriarPoltronasParaVoo(int vooId, int numeroPoltronas)
        {
            var poltronas = new List<Poltrona>();
            var random = new Random();

            var precos = new Dictionary<string, decimal>
            {
                ["Primeira"] = 800.00m,
                ["Executiva"] = 500.00m,
                ["Economica"] = 300.00m
            };

            for (int i = 1; i <= numeroPoltronas; i++)
            {
                var localizacao = (i % 3 == 0) ? "Corredor" :
                                 (i % 3 == 1) ? "Janela" : "Meio";

                var tipo = i <= (numeroPoltronas * 0.05) ? "Primeira" :
                           i <= (numeroPoltronas * 0.2) ? "Executiva" : "Economica";

                poltronas.Add(new Poltrona
                {
                    VooId = vooId,
                    NumeroPoltrona = i.ToString("D3"),
                    Disponivel = true,
                    Localizacao = localizacao,
                    Tipo = tipo,
                    Preco = precos[tipo]
                });
            }

            _context.Poltronas.AddRange(poltronas);
            await _context.SaveChangesAsync();
        }
    }
}