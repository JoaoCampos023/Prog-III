using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaAereo.Models;
using SistemaAereo.Repositories.Interfaces;

namespace SistemaAereo.Controllers
{
    public class AeroportosController : Controller
    {
        private readonly IAeroportoRepository _aeroportoRepository;
        private readonly ILogger<AeroportosController> _logger;

        public AeroportosController(
            IAeroportoRepository aeroportoRepository,
            ILogger<AeroportosController> logger)
        {
            _aeroportoRepository = aeroportoRepository;
            _logger = logger;
        }

        // =============================================
        // MÉTODOS PRINCIPAIS - CRUD
        // =============================================

        public async Task<IActionResult> Index()
        {
            try
            {
                var aeroportos = await _aeroportoRepository.GetAllAsync();
                return View(aeroportos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao carregar aeroportos");
                TempData["Erro"] = "Erro ao carregar lista de aeroportos";
                return View(new List<Aeroporto>());
            }
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Aeroporto aeroporto)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    if (await _aeroportoRepository.CodigoIATAExistsAsync(aeroporto.CodigoIATA))
                    {
                        ModelState.AddModelError("CodigoIATA", "Este código IATA já está cadastrado.");
                        return View(aeroporto);
                    }

                    await _aeroportoRepository.AddAsync(aeroporto);
                    TempData["Sucesso"] = "Aeroporto cadastrado com sucesso!";
                    return RedirectToAction(nameof(Index));
                }
                return View(aeroporto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao criar aeroporto");
                TempData["Erro"] = "Erro ao cadastrar aeroporto";
                return View(aeroporto);
            }
        }

        public async Task<IActionResult> Edit(int id)
        {
            try
            {
                var aeroporto = await _aeroportoRepository.GetByIdAsync(id);
                if (aeroporto == null)
                {
                    TempData["Erro"] = "Aeroporto não encontrado";
                    return RedirectToAction(nameof(Index));
                }
                return View(aeroporto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao carregar aeroporto para edição");
                TempData["Erro"] = "Erro ao carregar aeroporto";
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Aeroporto aeroporto)
        {
            try
            {
                if (id != aeroporto.AeroportoId)
                {
                    TempData["Erro"] = "ID do aeroporto inválido";
                    return RedirectToAction(nameof(Index));
                }

                if (ModelState.IsValid)
                {
                    if (await _aeroportoRepository.CodigoIATAExistsAsync(aeroporto.CodigoIATA, id))
                    {
                        ModelState.AddModelError("CodigoIATA", "Este código IATA já está cadastrado.");
                        return View(aeroporto);
                    }

                    await _aeroportoRepository.UpdateAsync(aeroporto);
                    TempData["Sucesso"] = "Aeroporto atualizado com sucesso!";
                    return RedirectToAction(nameof(Index));
                }
                return View(aeroporto);
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await _aeroportoRepository.ExistsAsync(a => a.AeroportoId == id))
                {
                    TempData["Erro"] = "Aeroporto não encontrado";
                    return RedirectToAction(nameof(Index));
                }
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao atualizar aeroporto");
                TempData["Erro"] = "Erro ao atualizar aeroporto";
                return View(aeroporto);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                if (await _aeroportoRepository.HasVoosAsync(id))
                {
                    TempData["Erro"] = "Não é possível excluir o aeroporto pois existem voos associados a ele.";
                    return RedirectToAction(nameof(Index));
                }

                var aeroporto = await _aeroportoRepository.GetByIdAsync(id);
                if (aeroporto != null)
                {
                    await _aeroportoRepository.DeleteAsync(aeroporto);
                    TempData["Sucesso"] = "Aeroporto excluído com sucesso!";
                }
                else
                {
                    TempData["Erro"] = "Aeroporto não encontrado";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao excluir aeroporto");
                TempData["Erro"] = "Erro ao excluir aeroporto";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}