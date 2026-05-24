using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SistemaAereo.Data.Context;
using SistemaAereo.Models.Entities;
using SistemaAereo.Models.Enums;
using SistemaAereo.Models.ViewModels;
using SistemaAereo.Services.Interfaces;

namespace SistemaAereo.Controllers
{
    public class VoosController : Controller
    {
        private readonly AeroportoContext _context;
        private readonly IPoltronaService _poltronaService;
        private readonly ILogger<VoosController> _logger;

        public VoosController(
            AeroportoContext context,
            IPoltronaService poltronaService,
            ILogger<VoosController> logger)
        {
            _context = context;
            _poltronaService = poltronaService;
            _logger = logger;
        }

        // =============================================
        // MÉTODOS PRINCIPAIS - CRUD
        // =============================================

        // GET: Voos (COM PAGINAÇÃO)
        public async Task<IActionResult> Index(int pagina = 1, int itensPorPagina = 10, string status = null)
        {
            try
            {
                var query = _context.Voos
                    .AsNoTracking()
                    .Include(v => v.AeroportoOrigem)
                    .Include(v => v.AeroportoDestino)
                    .Include(v => v.Aeronave)
                    .AsQueryable();

                // Filtro por status
                if (!string.IsNullOrEmpty(status))
                {
                    var agora = DateTime.Now;
                    query = status.ToLower() switch
                    {
                        "futuros" => query.Where(v => v.HorarioSaida > agora),
                        "hoje" => query.Where(v => v.HorarioSaida.Date == DateTime.Today),
                        "passados" => query.Where(v => v.HorarioSaida < agora),
                        _ => query
                    };
                    ViewBag.StatusFiltro = status;
                }

                var totalItens = await query.CountAsync();
                var voos = await query
                    .OrderBy(v => v.HorarioSaida)
                    .Skip((pagina - 1) * itensPorPagina)
                    .Take(itensPorPagina)
                    .ToListAsync();

                var model = new PaginacaoViewModel<Voo>(voos, totalItens, pagina, itensPorPagina);

                // Opções de paginação para a view
                ViewBag.ItensPorPaginaOptions = new[] { 5, 10, 25, 50, 100 };
                ViewBag.ItensPorPaginaAtual = itensPorPagina;
                ViewBag.StatusOptions = new[] { "futuros", "hoje", "passados" };

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao carregar voos");
                TempData["Erro"] = "Erro ao carregar lista de voos";

                // Garantir que os ViewBags sejam definidos mesmo em caso de erro
                ViewBag.ItensPorPaginaOptions = new[] { 5, 10, 25, 50, 100 };
                ViewBag.ItensPorPaginaAtual = itensPorPagina;
                ViewBag.StatusOptions = new[] { "futuros", "hoje", "passados" };

                return View(new PaginacaoViewModel<Voo>());
            }
        }

        // GET: Voos/Create
        public async Task<IActionResult> Create()
        {
            await CarregarViewBags();
            return View(new Voo());
        }

        // POST: Voos/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            string NumeroVoo,
            int AeroportoOrigemId,
            int AeroportoDestinoId,
            int AeronaveId,
            DateTime HorarioSaida,
            DateTime HorarioChegadaPrevisto)
        {
            try
            {
                _logger.LogInformation("=== DADOS RECEBIDOS VIA PARÂMETROS ===");
                _logger.LogInformation($"NumeroVoo: {NumeroVoo}");
                _logger.LogInformation($"AeroportoOrigemId: {AeroportoOrigemId}");
                _logger.LogInformation($"AeroportoDestinoId: {AeroportoDestinoId}");
                _logger.LogInformation($"AeronaveId: {AeronaveId}");
                _logger.LogInformation($"HorarioSaida: {HorarioSaida}");
                _logger.LogInformation($"HorarioChegadaPrevisto: {HorarioChegadaPrevisto}");

                // Criar objeto Voo manualmente
                var voo = new Voo
                {
                    NumeroVoo = NumeroVoo?.Trim().ToUpper(),
                    AeroportoOrigemId = AeroportoOrigemId,
                    AeroportoDestinoId = AeroportoDestinoId,
                    AeronaveId = AeronaveId,
                    HorarioSaida = HorarioSaida,
                    HorarioChegadaPrevisto = HorarioChegadaPrevisto
                };

                // Validar datas passadas
                if (HorarioSaida < DateTime.Now)
                {
                    ModelState.AddModelError("HorarioSaida", "Não é possível cadastrar um voo com data/hora no passado.");
                }

                if (HorarioChegadaPrevisto < DateTime.Now)
                {
                    ModelState.AddModelError("HorarioChegadaPrevisto", "Não é possível cadastrar um voo com chegada no passado.");
                }

                // Validações manuais
                if (AeroportoOrigemId == AeroportoDestinoId)
                {
                    ModelState.AddModelError("AeroportoDestinoId", "O aeroporto de destino deve ser diferente do aeroporto de origem.");
                }

                if (HorarioChegadaPrevisto <= HorarioSaida)
                {
                    ModelState.AddModelError("HorarioChegadaPrevisto", "O horário de chegada deve ser posterior ao horário de saída.");
                }

                var numeroVooExists = await _context.Voos
                    .AsNoTracking()
                    .AnyAsync(v => v.NumeroVoo == NumeroVoo);

                if (numeroVooExists)
                {
                    ModelState.AddModelError("NumeroVoo", "Este número de voo já está cadastrado.");
                }

                if (ModelState.IsValid)
                {
                    // Usar transação para garantir consistência
                    using var transaction = await _context.Database.BeginTransactionAsync();

                    try
                    {
                        _context.Voos.Add(voo);
                        await _context.SaveChangesAsync();

                        await _poltronaService.CriarPoltronasParaVooAsync(voo.VooId);

                        await transaction.CommitAsync();

                        TempData["Sucesso"] = $"Voo {voo.NumeroVoo} cadastrado com sucesso!";
                        return RedirectToAction(nameof(Index));
                    }
                    catch (Exception ex)
                    {
                        await transaction.RollbackAsync();
                        _logger.LogError(ex, "Erro na transação ao criar voo");
                        ModelState.AddModelError("", "Erro ao salvar o voo. Tente novamente.");
                    }
                }

                await CarregarViewBags();
                return View(voo);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ERRO ao criar voo");
                TempData["Erro"] = $"Erro: {ex.Message}";
                await CarregarViewBags();

                var voo = new Voo
                {
                    NumeroVoo = NumeroVoo,
                    AeroportoOrigemId = AeroportoOrigemId,
                    AeroportoDestinoId = AeroportoDestinoId,
                    AeronaveId = AeronaveId,
                    HorarioSaida = HorarioSaida,
                    HorarioChegadaPrevisto = HorarioChegadaPrevisto
                };

                return View(voo);
            }
        }

        // GET: Voos/Details/5
        public async Task<IActionResult> Details(int id)
        {
            try
            {
                var voo = await _context.Voos
                    .AsNoTracking()
                    .Include(v => v.AeroportoOrigem)
                    .Include(v => v.AeroportoDestino)
                    .Include(v => v.Aeronave)
                    .FirstOrDefaultAsync(v => v.VooId == id);

                if (voo == null)
                {
                    TempData["Erro"] = "Voo não encontrado";
                    return RedirectToAction(nameof(Index));
                }

                var totalPoltronas = await _poltronaService.GetTotalPoltronasDisponiveisAsync(id);
                var poltronasOcupadas = await _poltronaService.GetTotalPoltronasOcupadasAsync(id);

                ViewBag.TotalPoltronasDisponiveis = totalPoltronas;
                ViewBag.TotalPoltronasOcupadas = poltronasOcupadas;
                ViewBag.TotalPoltronas = totalPoltronas + poltronasOcupadas;

                return View(voo);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao carregar detalhes do voo {VooId}", id);
                TempData["Erro"] = "Erro ao carregar detalhes do voo";
                return RedirectToAction(nameof(Index));
            }
        }

        // GET: Voos/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            try
            {
                var voo = await _context.Voos
                    .FirstOrDefaultAsync(v => v.VooId == id);

                if (voo == null)
                {
                    TempData["Erro"] = "Voo não encontrado";
                    return RedirectToAction(nameof(Index));
                }

                await CarregarViewBags();
                return View(voo);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao carregar voo para edição");
                TempData["Erro"] = "Erro ao carregar voo";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Voos/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Voo voo)
        {
            if (id != voo.VooId)
            {
                TempData["Erro"] = "ID do voo inválido";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                if (voo.HorarioSaida < DateTime.Now)
                {
                    ModelState.AddModelError("HorarioSaida", "Não é possível editar para uma data/hora no passado.");
                }

                await ValidarVooAsync(voo);

                if (ModelState.IsValid)
                {
                    _context.Update(voo);
                    await _context.SaveChangesAsync();
                    TempData["Sucesso"] = "Voo atualizado com sucesso!";
                    return RedirectToAction(nameof(Index));
                }

                await CarregarViewBags();
                return View(voo);
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await VooExistsAsync(id))
                {
                    TempData["Erro"] = "Voo não encontrado";
                    return RedirectToAction(nameof(Index));
                }
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao atualizar voo");
                TempData["Erro"] = "Erro ao atualizar voo";
                await CarregarViewBags();
                return View(voo);
            }
        }

        // GET: Voos/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var voo = await _context.Voos
                    .AsNoTracking()
                    .Include(v => v.AeroportoOrigem)
                    .Include(v => v.AeroportoDestino)
                    .Include(v => v.Aeronave)
                    .FirstOrDefaultAsync(v => v.VooId == id);

                if (voo == null)
                {
                    TempData["Erro"] = "Voo não encontrado";
                    return RedirectToAction(nameof(Index));
                }

                var temPassagens = await _context.Passagens.AnyAsync(p => p.VooId == id && p.Status != PassagemStatus.Cancelada);
                if (temPassagens)
                {
                    TempData["Erro"] = "Não é possível excluir o voo pois existem passagens vendidas.";
                    return RedirectToAction(nameof(Index));
                }

                return View(voo);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao carregar voo para exclusão");
                TempData["Erro"] = "Erro ao carregar voo";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Voos/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                var voo = await _context.Voos
                    .Include(v => v.Poltronas)
                    .Include(v => v.Passagens)
                    .FirstOrDefaultAsync(v => v.VooId == id);

                if (voo == null)
                {
                    TempData["Erro"] = "Voo não encontrado";
                    return RedirectToAction(nameof(Index));
                }

                var temPassagens = voo.Passagens.Any(p => p.Status != PassagemStatus.Cancelada);
                if (temPassagens)
                {
                    TempData["Erro"] = "Não é possível excluir o voo pois existem passagens vendidas.";
                    return RedirectToAction(nameof(Index));
                }

                using var transaction = await _context.Database.BeginTransactionAsync();

                try
                {
                    if (voo.Poltronas.Any())
                    {
                        _context.Poltronas.RemoveRange(voo.Poltronas);
                    }

                    var escalas = await _context.Escalas.Where(e => e.VooId == id).ToListAsync();
                    if (escalas.Any())
                    {
                        _context.Escalas.RemoveRange(escalas);
                    }

                    _context.Voos.Remove(voo);
                    await _context.SaveChangesAsync();

                    await transaction.CommitAsync();

                    TempData["Sucesso"] = "Voo excluído com sucesso!";
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    _logger.LogError(ex, "Erro ao excluir voo");
                    TempData["Erro"] = "Erro ao excluir voo";
                }

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao excluir voo {VooId}", id);
                TempData["Erro"] = "Erro ao excluir voo";
                return RedirectToAction(nameof(Index));
            }
        }

        // =============================================
        // MÉTODOS ADICIONAIS
        // =============================================

        // GET: Voos/Poltronas/5
        public async Task<IActionResult> Poltronas(int id)
        {
            try
            {
                var voo = await _context.Voos
                    .AsNoTracking()
                    .Include(v => v.AeroportoOrigem)
                    .Include(v => v.AeroportoDestino)
                    .FirstOrDefaultAsync(v => v.VooId == id);

                if (voo == null)
                {
                    TempData["Erro"] = "Voo não encontrado";
                    return RedirectToAction(nameof(Index));
                }

                var poltronas = await _context.Poltronas
                    .AsNoTracking()
                    .Where(p => p.VooId == id)
                    .OrderBy(p => p.NumeroPoltrona)
                    .ToListAsync();

                ViewBag.Voo = voo;
                return View(poltronas);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao carregar poltronas do voo {VooId}", id);
                TempData["Erro"] = "Erro ao carregar poltronas";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Voos/RecriarPoltronas/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RecriarPoltronas(int id)
        {
            try
            {
                var voo = await _context.Voos
                    .Include(v => v.Poltronas)
                    .FirstOrDefaultAsync(v => v.VooId == id);

                if (voo == null)
                {
                    TempData["Erro"] = "Voo não encontrado";
                    return RedirectToAction(nameof(Index));
                }

                var temPassagens = await _context.Passagens
                    .AnyAsync(p => p.VooId == id && p.Status != PassagemStatus.Cancelada);

                if (temPassagens)
                {
                    TempData["Erro"] = "Não é possível recriar poltronas pois existem passagens vendidas para este voo.";
                    return RedirectToAction(nameof(Poltronas), new { id = id });
                }

                using var transaction = await _context.Database.BeginTransactionAsync();

                try
                {
                    if (voo.Poltronas.Any())
                    {
                        _context.Poltronas.RemoveRange(voo.Poltronas);
                        await _context.SaveChangesAsync();
                    }

                    await _poltronaService.CriarPoltronasParaVooAsync(id);

                    await transaction.CommitAsync();

                    TempData["Sucesso"] = "Poltronas recriadas com sucesso!";
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    _logger.LogError(ex, "Erro ao recriar poltronas");
                    TempData["Erro"] = "Erro ao recriar poltronas";
                }

                return RedirectToAction(nameof(Poltronas), new { id = id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao recriar poltronas do voo {VooId}", id);
                TempData["Erro"] = "Erro ao recriar poltronas";
                return RedirectToAction(nameof(Index));
            }
        }

        // =============================================
        // MÉTODOS PRIVADOS AUXILIARES
        // =============================================

        private async Task CarregarViewBags()
        {
            try
            {
                var aeroportos = await _context.Aeroportos
                    .AsNoTracking()
                    .OrderBy(a => a.Nome)
                    .ToListAsync();

                ViewBag.Aeroportos = aeroportos
                    .Select(a => new SelectListItem
                    {
                        Value = a.AeroportoId.ToString(),
                        Text = $"{a.Nome} ({a.CodigoIATA}) - {a.Cidade}"
                    })
                    .ToList();

                var aeronaves = await _context.Aeronaves
                    .AsNoTracking()
                    .OrderBy(a => a.TipoAeronave)
                    .ToListAsync();

                ViewBag.Aeronaves = aeronaves
                    .Select(a => new SelectListItem
                    {
                        Value = a.AeronaveId.ToString(),
                        Text = $"{a.TipoAeronave} - {a.NumeroPoltronas} poltronas"
                    })
                    .ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao carregar ViewBags");
                ViewBag.Aeroportos = new List<SelectListItem>();
                ViewBag.Aeronaves = new List<SelectListItem>();
            }
        }

        private async Task ValidarVooAsync(Voo voo)
        {
            if (voo.AeroportoOrigemId == voo.AeroportoDestinoId)
            {
                ModelState.AddModelError("AeroportoDestinoId", "O aeroporto de destino deve ser diferente do aeroporto de origem.");
            }

            if (voo.HorarioChegadaPrevisto <= voo.HorarioSaida)
            {
                ModelState.AddModelError("HorarioChegadaPrevisto", "O horário de chegada deve ser posterior ao horário de saída.");
            }

            var numeroVooExists = await _context.Voos
                .AsNoTracking()
                .AnyAsync(v => v.NumeroVoo == voo.NumeroVoo && v.VooId != voo.VooId);

            if (numeroVooExists)
            {
                ModelState.AddModelError("NumeroVoo", "Este número de voo já está cadastrado.");
            }
        }

        private async Task<bool> VooExistsAsync(int id)
        {
            return await _context.Voos.AnyAsync(v => v.VooId == id);
        }
    }
}