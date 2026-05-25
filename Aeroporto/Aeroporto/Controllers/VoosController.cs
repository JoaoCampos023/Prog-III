using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SistemaAereo.Data.Context;
using SistemaAereo.Facades.Interfaces;
using SistemaAereo.Models.Entities;
using SistemaAereo.Models.ViewModels;
using SistemaAereo.Facades.Interfaces;

namespace SistemaAereo.Controllers
{
    public class VoosController : Controller
    {
        private readonly IVooFacade _vooFacade;
        private readonly AeroportoContext _context;
        private readonly ILogger<VoosController> _logger;

        public VoosController(
            IVooFacade vooFacade,
            AeroportoContext context,
            ILogger<VoosController> logger)
        {
            _vooFacade = vooFacade;
            _context = context;
            _logger = logger;
        }

        // GET: Voos
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
                ViewBag.ItensPorPaginaOptions = new[] { 5, 10, 25, 50, 100 };
                ViewBag.ItensPorPaginaAtual = itensPorPagina;

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao carregar voos");
                TempData["Erro"] = "Erro ao carregar lista de voos";
                return View(new PaginacaoViewModel<Voo>());
            }
        }

        // GET: Voos/Create
        public async Task<IActionResult> Create()
        {
            await CarregarViewBags();
            return View(new Voo());
        }

        // POST: Voos/Create - USANDO FACADE
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Voo voo)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    await CarregarViewBags();
                    return View(voo);
                }

                var result = await _vooFacade.CriarVooAsync(voo);

                if (result.Success)
                {
                    TempData["Sucesso"] = result.Message;
                    return RedirectToAction(nameof(Index));
                }

                ModelState.AddModelError("", result.ErrorMessage);
                await CarregarViewBags();
                return View(voo);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao criar voo");
                TempData["Erro"] = "Erro ao criar voo";
                await CarregarViewBags();
                return View(voo);
            }
        }

        // GET: Voos/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var voo = await _context.Voos.FindAsync(id);
            if (voo == null)
            {
                TempData["Erro"] = "Voo não encontrado";
                return RedirectToAction(nameof(Index));
            }

            await CarregarViewBags();
            return View(voo);
        }

        // POST: Voos/Edit/5 - USANDO FACADE
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
                var result = await _vooFacade.AtualizarVooAsync(voo);

                if (result.Success)
                {
                    TempData["Sucesso"] = result.Message;
                    return RedirectToAction(nameof(Index));
                }

                ModelState.AddModelError("", result.ErrorMessage);
                await CarregarViewBags();
                return View(voo);
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
            var voo = await _context.Voos
                .Include(v => v.AeroportoOrigem)
                .Include(v => v.AeroportoDestino)
                .Include(v => v.Aeronave)
                .FirstOrDefaultAsync(v => v.VooId == id);

            if (voo == null)
            {
                TempData["Erro"] = "Voo não encontrado";
                return RedirectToAction(nameof(Index));
            }

            return View(voo);
        }

        // POST: Voos/Delete/5 - USANDO FACADE
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var result = await _vooFacade.ExcluirVooAsync(id);

            if (result.Success)
                TempData["Sucesso"] = result.Message;
            else
                TempData["Erro"] = result.ErrorMessage;

            return RedirectToAction(nameof(Index));
        }

        // GET: Voos/Poltronas/5
        public async Task<IActionResult> Poltronas(int id)
        {
            var voo = await _context.Voos
                .Include(v => v.AeroportoOrigem)
                .Include(v => v.AeroportoDestino)
                .FirstOrDefaultAsync(v => v.VooId == id);

            if (voo == null)
            {
                TempData["Erro"] = "Voo não encontrado";
                return RedirectToAction(nameof(Index));
            }

            var poltronas = await _context.Poltronas
                .Where(p => p.VooId == id)
                .OrderBy(p => p.NumeroPoltrona)
                .ToListAsync();

            var estatisticas = await _vooFacade.ObterEstatisticasVooAsync(id);
            ViewBag.Estatisticas = estatisticas;
            ViewBag.Voo = voo;

            return View(poltronas);
        }

        // POST: Voos/RecriarPoltronas/5 - USANDO FACADE
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RecriarPoltronas(int id)
        {
            var result = await _vooFacade.RecriarPoltronasAsync(id);

            if (result.Success)
                TempData["Sucesso"] = result.Message;
            else
                TempData["Erro"] = result.ErrorMessage;

            return RedirectToAction(nameof(Poltronas), new { id = id });
        }

        // =============================================
        // MÉTODOS PRIVADOS AUXILIARES
        // =============================================

        private async Task CarregarViewBags()
        {
            var aeroportos = await _context.Aeroportos
                .OrderBy(a => a.Nome)
                .ToListAsync();

            ViewBag.Aeroportos = new SelectList(aeroportos, "AeroportoId", "Nome", null, "Cidade");

            var aeronaves = await _context.Aeronaves
                .OrderBy(a => a.TipoAeronave)
                .ToListAsync();

            ViewBag.Aeronaves = new SelectList(aeronaves, "AeronaveId", "TipoAeronave");
        }
    }
}