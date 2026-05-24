using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaAereo.Models;
using SistemaAereo.Repositories.Interfaces;

namespace SistemaAereo.Controllers
{
    public class AeronavesController : Controller
    {
        private readonly IAeronaveRepository _aeronaveRepository;
        private readonly ILogger<AeronavesController> _logger;

        public AeronavesController(
            IAeronaveRepository aeronaveRepository,
            ILogger<AeronavesController> logger)
        {
            _aeronaveRepository = aeronaveRepository;
            _logger = logger;
        }

        // =============================================
        // MÉTODOS PRINCIPAIS - CRUD
        // =============================================

        public async Task<IActionResult> Index()
        {
            try
            {
                var aeronaves = await _aeronaveRepository.GetAeronavesComVoosAsync();
                return View(aeronaves);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao carregar aeronaves");
                TempData["Erro"] = "Erro ao carregar lista de aeronaves";
                return View(new List<Aeronave>());
            }
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Aeronave aeronave)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    await _aeronaveRepository.AddAsync(aeronave);
                    TempData["Sucesso"] = "Aeronave cadastrada com sucesso!";
                    return RedirectToAction(nameof(Index));
                }
                return View(aeronave);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao criar aeronave");
                TempData["Erro"] = "Erro ao cadastrar aeronave";
                return View(aeronave);
            }
        }

        public async Task<IActionResult> Edit(int id)
        {
            try
            {
                var aeronave = await _aeronaveRepository.GetByIdAsync(id);
                if (aeronave == null)
                {
                    TempData["Erro"] = "Aeronave não encontrada";
                    return RedirectToAction(nameof(Index));
                }
                return View(aeronave);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao carregar aeronave para edição");
                TempData["Erro"] = "Erro ao carregar aeronave";
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Aeronave aeronave)
        {
            try
            {
                if (id != aeronave.AeronaveId)
                {
                    TempData["Erro"] = "ID da aeronave inválido";
                    return RedirectToAction(nameof(Index));
                }

                if (ModelState.IsValid)
                {
                    await _aeronaveRepository.UpdateAsync(aeronave);
                    TempData["Sucesso"] = "Aeronave atualizada com sucesso!";
                    return RedirectToAction(nameof(Index));
                }
                return View(aeronave);
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await _aeronaveRepository.ExistsAsync(a => a.AeronaveId == id))
                {
                    TempData["Erro"] = "Aeronave não encontrada";
                    return RedirectToAction(nameof(Index));
                }
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao atualizar aeronave");
                TempData["Erro"] = "Erro ao atualizar aeronave";
                return View(aeronave);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                if (await _aeronaveRepository.HasVoosAsync(id))
                {
                    TempData["Erro"] = "Não é possível excluir a aeronave pois existem voos associados a ela.";
                    return RedirectToAction(nameof(Index));
                }

                var aeronave = await _aeronaveRepository.GetByIdAsync(id);
                if (aeronave != null)
                {
                    await _aeronaveRepository.DeleteAsync(aeronave);
                    TempData["Sucesso"] = "Aeronave excluída com sucesso!";
                }
                else
                {
                    TempData["Erro"] = "Aeronave não encontrada";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao excluir aeronave");
                TempData["Erro"] = "Erro ao excluir aeronave";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}