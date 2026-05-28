using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SistemaAereo.Data.Context;
using SistemaAereo.Facades.Interfaces;
using SistemaAereo.Models.DTOs;
using SistemaAereo.Models.Entities;
using SistemaAereo.Models.Enums;
using SistemaAereo.Models.ViewModels;

namespace SistemaAereo.Controllers
{
    [Authorize]
    public class TicketsController : Controller
    {
        private readonly ITicketFacade _ticketFacade;
        private readonly AirportsContext _context;
        private readonly ILogger<TicketsController> _logger;

        public TicketsController(
            ITicketFacade ticketFacade,
            AirportsContext context,
            ILogger<TicketsController> logger)
        {
            _ticketFacade = ticketFacade;
            _context = context;
            _logger = logger;
        }

        // =============================================
        // MÉTODOS PRINCIPAIS
        // =============================================

        /// <summary>
        /// Lista todas as passagens com paginação
        /// </summary>
        public async Task<IActionResult> Index(int page = 1, int itemsPerPage = 10, string status = null)
        {
            try
            {
                var query = _context.Tickets
                    .AsNoTracking()
                    .Include(t => t.Flight)
                        .ThenInclude(f => f.DepartureAirport)
                    .Include(t => t.Flight)
                        .ThenInclude(f => f.ArrivalAirport)
                    .Include(t => t.Customer)
                    .Include(t => t.Seat)
                    .AsQueryable();

                if (!string.IsNullOrEmpty(status) && TicketStatus.IsValid(status))
                {
                    query = query.Where(t => t.Status == status);
                    ViewBag.StatusFilter = status;
                }

                var totalItems = await query.CountAsync();
                var tickets = await query
                    .OrderByDescending(t => t.IssueDate)
                    .Skip((page - 1) * itemsPerPage)
                    .Take(itemsPerPage)
                    .ToListAsync();

                var model = new PaginationViewModel<Ticket>(tickets, totalItems, page, itemsPerPage);

                ViewBag.ItemsPerPageOptions = new[] { 5, 10, 25, 50, 100 };
                ViewBag.CurrentItemsPerPage = itemsPerPage;
                ViewBag.StatusOptions = TicketStatus.GetAll();

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao carregar passagens");
                TempData["Erro"] = "Erro ao carregar lista de passagens";
                return View(new PaginationViewModel<Ticket>());
            }
        }

        /// <summary>
        /// Detalhes de uma passagem
        /// </summary>
        public async Task<IActionResult> Details(int id)
        {
            try
            {
                var ticket = await _ticketFacade.GetTicketCompleteAsync(id);
                if (ticket == null)
                {
                    TempData["Erro"] = "Passagem não encontrada";
                    return RedirectToAction(nameof(Index));
                }
                return View(ticket);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao carregar detalhes da passagem {TicketId}", id);
                TempData["Erro"] = "Erro ao carregar passagem";
                return RedirectToAction(nameof(Index));
            }
        }

        // =============================================
        // EMISSÃO DE PASSAGENS
        // =============================================

        /// <summary>
        /// Formulário de emissão de passagem
        /// </summary>
        public async Task<IActionResult> Create()
        {
            await LoadViewBags();
            return View();
        }

        /// <summary>
        /// Emite uma nova passagem
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(IssueTicketRequestDto request)
        {
            try
            {
                _logger.LogInformation("=== TENTATIVA DE CRIAÇÃO DE PASSAGEM VIA FACADE ===");

                if (!ModelState.IsValid)
                {
                    await LoadViewBags();
                    return View(request);
                }

                var result = await _ticketFacade.IssueTicketAsync(request);

                if (result.Success)
                {
                    TempData["Sucesso"] = $"Passagem emitida com sucesso! Número: {result.TicketNumber}";
                    return RedirectToAction(nameof(Details), new { id = result.TicketId });
                }

                ModelState.AddModelError("", result.ErrorMessage);
                await LoadViewBags();
                return View(request);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao criar passagem");
                TempData["Erro"] = "Erro ao criar passagem";
                await LoadViewBags();
                return View(request);
            }
        }

        // =============================================
        // OPERAÇÕES DE PASSAGEM
        // =============================================

        /// <summary>
        /// Realiza check-in
        /// </summary>
        public async Task<IActionResult> Checkin(int id)
        {
            var result = await _ticketFacade.CheckinAsync(new CheckinRequestDto { TicketId = id });

            if (result.Success)
                TempData["Sucesso"] = result.Message;
            else
                TempData["Erro"] = result.ErrorMessage;

            return RedirectToAction(nameof(Details), new { id = id });
        }

        /// <summary>
        /// Registra embarque
        /// </summary>
        public async Task<IActionResult> Boarding(int id)
        {
            var result = await _ticketFacade.RegisterBoardingAsync(id);

            if (result.Success)
                TempData["Sucesso"] = result.Message;
            else
                TempData["Erro"] = result.ErrorMessage;

            return RedirectToAction(nameof(Details), new { id = id });
        }

        /// <summary>
        /// Cancela uma passagem
        /// </summary>
        public async Task<IActionResult> Cancel(int id)
        {
            var result = await _ticketFacade.CancelTicketAsync(new CancelTicketRequestDto { TicketId = id });

            if (result.Success)
                TempData["Sucesso"] = result.Message;
            else
                TempData["Erro"] = result.ErrorMessage;

            return RedirectToAction(nameof(Index));
        }

        // =============================================
        // CONSULTAS ESPECÍFICAS
        // =============================================

        /// <summary>
        /// Passagens por cliente
        /// </summary>
        public async Task<IActionResult> ByCustomer(int id, int page = 1, int itemsPerPage = 10)
        {
            try
            {
                var customer = await _context.Customers.FindAsync(id);
                if (customer == null)
                {
                    TempData["Erro"] = "Cliente não encontrado";
                    return RedirectToAction(nameof(Index));
                }

                ViewBag.CustomerName = customer.Name;
                ViewBag.CustomerId = id;

                var query = _context.Tickets
                    .AsNoTracking()
                    .Include(t => t.Flight)
                        .ThenInclude(f => f.DepartureAirport)
                    .Include(t => t.Flight)
                        .ThenInclude(f => f.ArrivalAirport)
                    .Include(t => t.Seat)
                    .Where(t => t.CustomerId == id);

                var totalItems = await query.CountAsync();
                var tickets = await query
                    .OrderByDescending(t => t.IssueDate)
                    .Skip((page - 1) * itemsPerPage)
                    .Take(itemsPerPage)
                    .ToListAsync();

                var model = new PaginationViewModel<Ticket>(tickets, totalItems, page, itemsPerPage);
                ViewBag.ItemsPerPageOptions = new[] { 5, 10, 25, 50, 100 };
                ViewBag.CurrentItemsPerPage = itemsPerPage;

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao carregar passagens do cliente {CustomerId}", id);
                TempData["Erro"] = "Erro ao carregar passagens do cliente";
                return View(new PaginationViewModel<Ticket>());
            }
        }

        /// <summary>
        /// Passagens por voo
        /// </summary>
        public async Task<IActionResult> ByFlight(int id, int page = 1, int itemsPerPage = 10)
        {
            try
            {
                var flight = await _context.Flights.FindAsync(id);
                if (flight == null)
                {
                    TempData["Erro"] = "Voo não encontrado";
                    return RedirectToAction(nameof(Index));
                }

                ViewBag.FlightNumber = flight.FlightNumber;
                ViewBag.FlightId = id;

                var query = _context.Tickets
                    .AsNoTracking()
                    .Include(t => t.Customer)
                    .Include(t => t.Seat)
                    .Where(t => t.FlightId == id);

                var totalItems = await query.CountAsync();
                var tickets = await query
                    .OrderBy(t => t.Seat.SeatNumber)
                    .Skip((page - 1) * itemsPerPage)
                    .Take(itemsPerPage)
                    .ToListAsync();

                var model = new PaginationViewModel<Ticket>(tickets, totalItems, page, itemsPerPage);
                ViewBag.ItemsPerPageOptions = new[] { 5, 10, 25, 50, 100 };
                ViewBag.CurrentItemsPerPage = itemsPerPage;

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao carregar passagens do voo {FlightId}", id);
                TempData["Erro"] = "Erro ao carregar passagens do voo";
                return View(new PaginationViewModel<Ticket>());
            }
        }

        // =============================================
        // MÉTODOS AJAX
        // =============================================

        /// <summary>
        /// Busca poltronas disponíveis via AJAX
        /// </summary>
        public async Task<JsonResult> GetAvailableSeats(int flightId)
        {
            try
            {
                var seats = await _context.Seats
                    .AsNoTracking()
                    .Where(s => s.FlightId == flightId && s.IsAvailable)
                    .OrderBy(s => s.SeatNumber)
                    .ToListAsync();

                var result = seats.Select(s => new
                {
                    seatId = s.SeatId,
                    seatNumber = s.SeatNumber,
                    seatClass = s.Class,
                    location = s.Location,
                    price = s.Price
                }).ToList();

                return Json(new { success = true, data = result });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao carregar poltronas para voo {FlightId}", flightId);
                return Json(new { success = false, message = "Erro ao carregar poltronas" });
            }
        }

        // =============================================
        // MÉTODOS PRIVADOS
        // =============================================

        private async Task LoadViewBags()
        {
            var activeCustomers = await _context.Customers
                .Where(c => c.IsActive)
                .OrderBy(c => c.Name)
                .ToListAsync();

            ViewBag.Customers = new SelectList(activeCustomers, "CustomerId", "Name");

            var now = DateTime.Now;
            var availableFlights = await _context.Flights
                .Include(f => f.DepartureAirport)
                .Include(f => f.ArrivalAirport)
                .Include(f => f.Seats)
                .Where(f => f.DepartureTime > now && f.Seats.Any(s => s.IsAvailable))
                .OrderBy(f => f.DepartureTime)
                .ToListAsync();

            if (availableFlights.Any())
            {
                var flightsSelectList = availableFlights.Select(f => new
                {
                    FlightId = f.FlightId,
                    DisplayText = $"{f.FlightNumber} - {f.DepartureAirport?.IATACode ?? "N/A"} → {f.ArrivalAirport?.IATACode ?? "N/A"} - {f.DepartureTime:dd/MM/yyyy HH:mm}"
                }).ToList();

                ViewBag.FlightsDetails = new SelectList(flightsSelectList, "FlightId", "DisplayText");
            }
            else
            {
                ViewBag.FlightsDetails = new SelectList(new List<object>(), "FlightId", "DisplayText");
            }
        }
    }
}