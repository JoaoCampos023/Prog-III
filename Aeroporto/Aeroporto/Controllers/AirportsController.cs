using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaAereo.Data.Context;
using SistemaAereo.Models.Entities;
using SistemaAereo.Repositories.Interfaces;

namespace SistemaAereo.Controllers
{
    [Authorize]
    public class AirportsController : Controller
    {
        private readonly IAirportRepository _airportRepository;
        private readonly ILogger<AirportsController> _logger;

        public AirportsController(
            IAirportRepository airportRepository,
            ILogger<AirportsController> logger)
        {
            _airportRepository = airportRepository;
            _logger = logger;
        }

        /// <summary>
        /// Lista todos os aeroportos
        /// </summary>
        public async Task<IActionResult> Index()
        {
            try
            {
                var airports = await _airportRepository.GetAllAsync();
                return View(airports);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao carregar aeroportos");
                TempData["Erro"] = "Erro ao carregar lista de aeroportos";
                return View(new List<Airport>());
            }
        }

        /// <summary>
        /// Formulário de criação de aeroporto
        /// </summary>
        public IActionResult Create()
        {
            return View();
        }

        /// <summary>
        /// Cria um novo aeroporto
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Airport airport)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    if (await _airportRepository.IATACodeExistsAsync(airport.IATACode))
                    {
                        ModelState.AddModelError("IATACode", "Este código IATA já está cadastrado.");
                        return View(airport);
                    }

                    await _airportRepository.AddAsync(airport);
                    TempData["Sucesso"] = "Aeroporto cadastrado com sucesso!";
                    return RedirectToAction(nameof(Index));
                }
                return View(airport);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao criar aeroporto");
                TempData["Erro"] = "Erro ao cadastrar aeroporto";
                return View(airport);
            }
        }

        /// <summary>
        /// Formulário de edição de aeroporto
        /// </summary>
        public async Task<IActionResult> Edit(int id)
        {
            try
            {
                var airport = await _airportRepository.GetByIdAsync(id);
                if (airport == null)
                {
                    TempData["Erro"] = "Aeroporto não encontrado";
                    return RedirectToAction(nameof(Index));
                }
                return View(airport);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao carregar aeroporto para edição");
                TempData["Erro"] = "Erro ao carregar aeroporto";
                return RedirectToAction(nameof(Index));
            }
        }

        /// <summary>
        /// Atualiza um aeroporto
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Airport airport)
        {
            try
            {
                if (id != airport.AirportId)
                {
                    TempData["Erro"] = "ID do aeroporto inválido";
                    return RedirectToAction(nameof(Index));
                }

                if (ModelState.IsValid)
                {
                    if (await _airportRepository.IATACodeExistsAsync(airport.IATACode, id))
                    {
                        ModelState.AddModelError("IATACode", "Este código IATA já está cadastrado.");
                        return View(airport);
                    }

                    await _airportRepository.UpdateAsync(airport);
                    TempData["Sucesso"] = "Aeroporto atualizado com sucesso!";
                    return RedirectToAction(nameof(Index));
                }
                return View(airport);
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await _airportRepository.ExistsAsync(a => a.AirportId == id))
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
                return View(airport);
            }
        }

        /// <summary>
        /// Exclui um aeroporto
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                if (await _airportRepository.HasFlightsAsync(id))
                {
                    TempData["Erro"] = "Não é possível excluir o aeroporto pois existem voos associados a ele.";
                    return RedirectToAction(nameof(Index));
                }

                var airport = await _airportRepository.GetByIdAsync(id);
                if (airport != null)
                {
                    await _airportRepository.DeleteAsync(airport);
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