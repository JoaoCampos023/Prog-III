using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SistemaAereo.Data.Context;
using SistemaAereo.Facades.Interfaces;
using SistemaAereo.Models.Entities;
using SistemaAereo.Models.ViewModels;

namespace SistemaAereo.Controllers
{
    [Authorize]
    public class FlightsController : Controller
    {
        private readonly IFlightFacade _flightFacade;
        private readonly AirportsContext _context;
        private readonly ILogger<FlightsController> _logger;

        public FlightsController(
            IFlightFacade flightFacade,
            AirportsContext context,
            ILogger<FlightsController> logger)
        {
            _flightFacade = flightFacade;
            _context = context;
            _logger = logger;
        }

        // =============================================
        // MÉTODOS PRINCIPAIS
        // =============================================

        // Lista todos os voos com paginação
        public async Task<IActionResult> Index(int page = 1, int itemsPerPage = 10, string status = null)
        {
            try
            {
                var query = _context.Flights
                    .AsNoTracking()
                    .Include(f => f.DepartureAirport)
                    .Include(f => f.ArrivalAirport)
                    .Include(f => f.Aircraft)
                    .AsQueryable();

                // Aplica filtro por status (futuros, hoje, passados)
                if (!string.IsNullOrEmpty(status))
                {
                    var now = DateTime.Now;
                    query = status.ToLower() switch
                    {
                        "upcoming" => query.Where(f => f.DepartureTime > now),
                        "today" => query.Where(f => f.DepartureTime.Date == DateTime.Today),
                        "past" => query.Where(f => f.DepartureTime < now),
                        _ => query
                    };
                    ViewBag.StatusFilter = status;
                }

                // Contagem total para paginação
                var totalItems = await query.CountAsync();
                var flights = await query
                    .OrderBy(f => f.DepartureTime)
                    .Skip((page - 1) * itemsPerPage)
                    .Take(itemsPerPage)
                    .ToListAsync();

                var model = new PaginationViewModel<Flight>(flights, totalItems, page, itemsPerPage);
                ViewBag.ItemsPerPageOptions = new[] { 5, 10, 25, 50, 100 };
                ViewBag.CurrentItemsPerPage = itemsPerPage;
                ViewBag.StatusOptions = new[] { "upcoming", "today", "past" };

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao carregar voos");
                TempData["Erro"] = "Erro ao carregar lista de voos";
                return View(new PaginationViewModel<Flight>());
            }
        }

        // Detalhes de um voo específico
        public async Task<IActionResult> Details(int id)
        {
            try
            {
                var flight = await _context.Flights
                    .AsNoTracking()
                    .Include(f => f.DepartureAirport)
                    .Include(f => f.ArrivalAirport)
                    .Include(f => f.Aircraft)
                    .FirstOrDefaultAsync(f => f.FlightId == id);

                if (flight == null)
                {
                    TempData["Erro"] = "Voo não encontrado";
                    return RedirectToAction(nameof(Index));
                }

                // Busca estatísticas do voo (poltronas disponíveis, ocupadas, etc.)
                var statistics = await _flightFacade.GetFlightStatisticsAsync(id);
                ViewBag.Statistics = statistics;

                return View(flight);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao carregar detalhes do voo {FlightId}", id);
                TempData["Erro"] = "Erro ao carregar detalhes do voo";
                return RedirectToAction(nameof(Index));
            }
        }

        // =============================================
        // CRIAÇÃO DE VOOS
        // =============================================

        // Formulário de criação de voo
        public async Task<IActionResult> Create()
        {
            await LoadViewBags();
            return View(new Flight());
        }

        // Processa a criação de um novo voo
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Flight flight)
        {
            try
            {
                // Remove campos de navegação da validação
                ModelState.Remove("DepartureAirport");
                ModelState.Remove("ArrivalAirport");
                ModelState.Remove("Aircraft");
                ModelState.Remove("Stopovers");
                ModelState.Remove("Seats");
                ModelState.Remove("Tickets");

                if (!ModelState.IsValid)
                {
                    await LoadViewBags();
                    return View(flight);
                }

                var result = await _flightFacade.CreateFlightAsync(flight);

                if (result.Success)
                {
                    TempData["Sucesso"] = result.Message;
                    return RedirectToAction(nameof(Index));
                }

                ModelState.AddModelError("", result.ErrorMessage);
                await LoadViewBags();
                return View(flight);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao criar voo");
                TempData["Erro"] = "Erro ao criar voo";
                await LoadViewBags();
                return View(flight);
            }
        }

        // =============================================
        // EDIÇÃO DE VOOS
        // =============================================

        // Formulário de edição de voo
        public async Task<IActionResult> Edit(int id)
        {
            var flight = await _context.Flights.FindAsync(id);
            if (flight == null)
            {
                TempData["Erro"] = "Voo não encontrado";
                return RedirectToAction(nameof(Index));
            }

            await LoadViewBags();
            return View(flight);
        }

        // Processa a atualização de um voo
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Flight flight)
        {
            if (id != flight.FlightId)
            {
                TempData["Erro"] = "ID do voo inválido";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                // Remove campos de navegação da validação
                ModelState.Remove("DepartureAirport");
                ModelState.Remove("ArrivalAirport");
                ModelState.Remove("Aircraft");
                ModelState.Remove("Stopovers");
                ModelState.Remove("Seats");
                ModelState.Remove("Tickets");

                var result = await _flightFacade.UpdateFlightAsync(flight);

                if (result.Success)
                {
                    TempData["Sucesso"] = result.Message;
                    return RedirectToAction(nameof(Index));
                }

                ModelState.AddModelError("", result.ErrorMessage);
                await LoadViewBags();
                return View(flight);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao atualizar voo");
                TempData["Erro"] = "Erro ao atualizar voo";
                await LoadViewBags();
                return View(flight);
            }
        }

        // =============================================
        // EXCLUSÃO DE VOOS
        // =============================================

        // Formulário de confirmação de exclusão
        public async Task<IActionResult> Delete(int id)
        {
            var flight = await _context.Flights
                .Include(f => f.DepartureAirport)
                .Include(f => f.ArrivalAirport)
                .Include(f => f.Aircraft)
                .FirstOrDefaultAsync(f => f.FlightId == id);

            if (flight == null)
            {
                TempData["Erro"] = "Voo não encontrado";
                return RedirectToAction(nameof(Index));
            }

            return View(flight);
        }

        // Confirma a exclusão do voo
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var result = await _flightFacade.DeleteFlightAsync(id);

            if (result.Success)
                TempData["Sucesso"] = result.Message;
            else
                TempData["Erro"] = result.ErrorMessage;

            return RedirectToAction(nameof(Index));
        }

        // =============================================
        // GERENCIAMENTO DE POLTRONAS
        // =============================================

        // Visualiza o mapa de poltronas de um voo
        public async Task<IActionResult> Seats(int id)
        {
            var flight = await _context.Flights
                .Include(f => f.DepartureAirport)
                .Include(f => f.ArrivalAirport)
                .FirstOrDefaultAsync(f => f.FlightId == id);

            if (flight == null)
            {
                TempData["Erro"] = "Voo não encontrado";
                return RedirectToAction(nameof(Index));
            }

            var seats = await _context.Seats
                .Where(s => s.FlightId == id)
                .OrderBy(s => s.SeatNumber)
                .ToListAsync();

            var statistics = await _flightFacade.GetFlightStatisticsAsync(id);
            ViewBag.Statistics = statistics;
            ViewBag.Flight = flight;

            return View(seats);
        }

        // Recria todas as poltronas de um voo (útil se houver erro na criação)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RecreateSeats(int id)
        {
            var result = await _flightFacade.RecreateSeatsAsync(id);

            if (result.Success)
                TempData["Sucesso"] = result.Message;
            else
                TempData["Erro"] = result.ErrorMessage;

            return RedirectToAction(nameof(Seats), new { id = id });
        }

        // =============================================
        // MÉTODOS PRIVADOS
        // =============================================

        // Carrega os dados para os dropdowns da view
        private async Task LoadViewBags()
        {
            // Lista de aeroportos para o dropdown
            var airports = await _context.Airports
                .OrderBy(a => a.Name)
                .ToListAsync();

            ViewBag.Airports = airports
                .Select(a => new SelectListItem
                {
                    Value = a.AirportId.ToString(),
                    Text = $"{a.Name} ({a.IATACode}) - {a.City}"
                })
                .ToList();

            // Lista de aeronaves para o dropdown
            var aircrafts = await _context.Aircrafts
                .OrderBy(a => a.AircraftType)
                .ToListAsync();

            ViewBag.Aircrafts = aircrafts
                .Select(a => new SelectListItem
                {
                    Value = a.AircraftId.ToString(),
                    Text = $"{a.AircraftType} - {a.NumberOfSeats} assentos"
                })
                .ToList();
        }
    }
}