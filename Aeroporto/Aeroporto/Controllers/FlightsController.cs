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

        // =============================================
        // CREATE - CORRIGIDO
        // =============================================

        public async Task<IActionResult> Create()
        {
            await LoadViewBags();
            return View(new Flight());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Flight flight)
        {
            try
            {
                // Remover validação de campos que não estão no formulário
                ModelState.Remove("DepartureAirport");
                ModelState.Remove("ArrivalAirport");
                ModelState.Remove("Aircraft");
                ModelState.Remove("Stopovers");
                ModelState.Remove("Seats");
                ModelState.Remove("Tickets");

                if (ModelState.IsValid)
                {
                    var result = await _flightFacade.CreateFlightAsync(flight);

                    if (result.Success)
                    {
                        TempData["Sucesso"] = result.Message;
                        return RedirectToAction(nameof(Index));
                    }

                    ModelState.AddModelError("", result.ErrorMessage);
                }

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
        // LOAD VIEW BAGS - CORRIGIDO
        // =============================================

        private async Task LoadViewBags()
        {
            try
            {
                // Carregar aeroportos
                var airports = await _context.Airports
                    .OrderBy(a => a.Name)
                    .Select(a => new SelectListItem
                    {
                        Value = a.AirportId.ToString(),
                        Text = $"{a.Name} ({a.IATACode}) - {a.City}"
                    })
                    .ToListAsync();

                ViewBag.Airports = airports;
                ViewBag.AirportsList = new SelectList(airports, "Value", "Text");

                // Carregar aeronaves
                var aircrafts = await _context.Aircrafts
                    .OrderBy(a => a.AircraftType)
                    .Select(a => new SelectListItem
                    {
                        Value = a.AircraftId.ToString(),
                        Text = $"{a.AircraftType} - {a.NumberOfSeats} assentos"
                    })
                    .ToListAsync();

                ViewBag.Aircrafts = aircrafts;
                ViewBag.AircraftsList = new SelectList(aircrafts, "Value", "Text");

                _logger.LogInformation($"ViewBags carregados: {airports.Count} aeroportos, {aircrafts.Count} aeronaves");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao carregar ViewBags");
                ViewBag.Airports = new List<SelectListItem>();
                ViewBag.Aircrafts = new List<SelectListItem>();
            }
        }

        // =============================================
        // OUTROS MÉTODOS
        // =============================================

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
                // Remover validação de campos de navegação
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

        public async Task<IActionResult> Details(int id)
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

            ViewBag.Flight = flight;
            return View(seats);
        }

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
    }
}