using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaAereo.Data.Context;
using SistemaAereo.Models.Entities;
using SistemaAereo.Repositories.Interfaces;

namespace SistemaAereo.Controllers
{
    [Authorize]
    public class AircraftsController : Controller
    {
        private readonly IAircraftRepository _aircraftRepository;
        private readonly ILogger<AircraftsController> _logger;

        public AircraftsController(
            IAircraftRepository aircraftRepository,
            ILogger<AircraftsController> logger)
        {
            _aircraftRepository = aircraftRepository;
            _logger = logger;
        }

        /// <summary>
        /// Lista todas as aeronaves
        /// </summary>
        public async Task<IActionResult> Index()
        {
            try
            {
                var aircrafts = await _aircraftRepository.GetAircraftsWithFlightsAsync();
                return View(aircrafts);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao carregar aeronaves");
                TempData["Erro"] = "Erro ao carregar lista de aeronaves";
                return View(new List<Aircraft>());
            }
        }

        /// <summary>
        /// Formulário de criação de aeronave
        /// </summary>
        public IActionResult Create()
        {
            return View();
        }

        /// <summary>
        /// Cria uma nova aeronave
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Aircraft aircraft)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    await _aircraftRepository.AddAsync(aircraft);
                    TempData["Sucesso"] = "Aeronave cadastrada com sucesso!";
                    return RedirectToAction(nameof(Index));
                }
                return View(aircraft);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao criar aeronave");
                TempData["Erro"] = "Erro ao cadastrar aeronave";
                return View(aircraft);
            }
        }

        /// <summary>
        /// Formulário de edição de aeronave
        /// </summary>
        public async Task<IActionResult> Edit(int id)
        {
            try
            {
                var aircraft = await _aircraftRepository.GetByIdAsync(id);
                if (aircraft == null)
                {
                    TempData["Erro"] = "Aeronave não encontrada";
                    return RedirectToAction(nameof(Index));
                }
                return View(aircraft);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao carregar aeronave para edição");
                TempData["Erro"] = "Erro ao carregar aeronave";
                return RedirectToAction(nameof(Index));
            }
        }

        /// <summary>
        /// Atualiza uma aeronave
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Aircraft aircraft)
        {
            try
            {
                if (id != aircraft.AircraftId)
                {
                    TempData["Erro"] = "ID da aeronave inválido";
                    return RedirectToAction(nameof(Index));
                }

                if (ModelState.IsValid)
                {
                    await _aircraftRepository.UpdateAsync(aircraft);
                    TempData["Sucesso"] = "Aeronave atualizada com sucesso!";
                    return RedirectToAction(nameof(Index));
                }
                return View(aircraft);
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await _aircraftRepository.ExistsAsync(a => a.AircraftId == id))
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
                return View(aircraft);
            }
        }

        /// <summary>
        /// Exclui uma aeronave
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                if (await _aircraftRepository.HasFlightsAsync(id))
                {
                    TempData["Erro"] = "Não é possível excluir a aeronave pois existem voos associados a ela.";
                    return RedirectToAction(nameof(Index));
                }

                var aircraft = await _aircraftRepository.GetByIdAsync(id);
                if (aircraft != null)
                {
                    await _aircraftRepository.DeleteAsync(aircraft);
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