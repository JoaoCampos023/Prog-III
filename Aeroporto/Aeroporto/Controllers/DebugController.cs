using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaAereo.Data.Context;
using SistemaAereo.Models.Entities;
using SistemaAereo.Services.Interfaces;

namespace SistemaAereo.Controllers
{
    public class DebugController : Controller
    {
        private readonly AeroportoContext _context;
        private readonly IPoltronaService _poltronaService;
        private readonly ILogger<DebugController> _logger;

        public DebugController(
            AeroportoContext context,
            IPoltronaService poltronaService,
            ILogger<DebugController> logger)
        {
            _context = context;
            _poltronaService = poltronaService;
            _logger = logger;
        }

        // =============================================
        // MÉTODOS DE VISUALIZAÇÃO DE DADOS
        // =============================================

        public async Task<IActionResult> Dados()
        {
            var dados = new
            {
                Voos = await _context.Voos
                    .AsNoTracking()
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
                    .AsNoTracking()
                    .Where(c => c.Ativo)
                    .Select(c => new { c.ClienteId, c.Nome })
                    .ToListAsync(),

                Aeronaves = await _context.Aeronaves
                    .AsNoTracking()
                    .Select(a => new { a.AeronaveId, a.TipoAeronave })
                    .ToListAsync(),

                Aeroportos = await _context.Aeroportos
                    .AsNoTracking()
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
                    .AsNoTracking()
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

                ClientesAtivos = await _context.ClientesPreferenciais
                    .AsNoTracking()
                    .CountAsync(c => c.Ativo)
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

                using var transaction = await _context.Database.BeginTransactionAsync();

                try
                {
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

                    // CORREÇÃO: Usar o PoltronaService centralizado
                    foreach (var voo in voos)
                    {
                        await _poltronaService.CriarPoltronasParaVooAsync(voo.VooId, aeronave.NumeroPoltronas);
                    }

                    await transaction.CommitAsync();

                    return Json(new
                    {
                        sucesso = true,
                        mensagem = "Dados de teste criados com sucesso! Foram criados 2 voos, 2 clientes e poltronas."
                    });
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao criar dados de teste");
                return Json(new { sucesso = false, mensagem = $"Erro ao criar dados de teste: {ex.Message}" });
            }
        }
    }
}