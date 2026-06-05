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

        // Lista todas as passagens com paginação
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

                // Se for usuário comum (User), mostrar apenas suas próprias passagens
                if (User.IsInRole("User") && !User.IsInRole("Admin") && !User.IsInRole("Funcionario"))
                {
                    var userEmail = User.Identity.Name;
                    var customer = await _context.Customers.FirstOrDefaultAsync(c => c.Email == userEmail);
                    if (customer != null)
                    {
                        query = query.Where(t => t.CustomerId == customer.CustomerId);
                    }
                }

                // Aplica filtro por status se informado
                if (!string.IsNullOrEmpty(status) && TicketStatus.IsValid(status))
                {
                    query = query.Where(t => t.Status == status);
                    ViewBag.StatusFilter = status;
                }

                // Contagem total para paginação
                var totalItems = await query.CountAsync();
                var tickets = await query
                    .OrderByDescending(t => t.IssueDate)
                    .Skip((page - 1) * itemsPerPage)
                    .Take(itemsPerPage)
                    .ToListAsync();

                var model = new PaginationViewModel<Ticket>(tickets, totalItems, page, itemsPerPage);

                // Configurações para a view
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

        // Detalhes de uma passagem - User pode ver apenas suas próprias
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

                // Verifica permissão para usuário comum
                if (!User.IsInRole("Admin") && !User.IsInRole("Funcionario"))
                {
                    var userEmail = User.Identity.Name;
                    var customer = await _context.Customers.FirstOrDefaultAsync(c => c.Email == userEmail);
                    if (customer == null || ticket.CustomerId != customer.CustomerId)
                    {
                        TempData["Erro"] = "Você não tem permissão para ver esta passagem.";
                        return RedirectToAction(nameof(Index));
                    }
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

        // Gera uma versão isolada do ticket para impressão
        [HttpGet]
        public async Task<IActionResult> PrintTicket(int id)
        {
            try
            {
                var ticket = await _ticketFacade.GetTicketCompleteAsync(id);
                if (ticket == null)
                {
                    TempData["Erro"] = "Passagem não encontrada";
                    return RedirectToAction(nameof(Index));
                }

                // Verifica permissão para usuário comum
                if (!User.IsInRole("Admin") && !User.IsInRole("Funcionario"))
                {
                    var userEmail = User.Identity.Name;
                    var customer = await _context.Customers.FirstOrDefaultAsync(c => c.Email == userEmail);
                    if (customer == null || ticket.CustomerId != customer.CustomerId)
                    {
                        TempData["Erro"] = "Você não tem permissão para imprimir esta passagem.";
                        return RedirectToAction(nameof(Index));
                    }
                }

                return View(ticket);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao carregar ticket para impressão");
                TempData["Erro"] = "Erro ao carregar passagem para impressão";
                return RedirectToAction(nameof(Index));
            }
        }

        // Formulário de emissão de passagem - apenas Admin e Funcionario
        [Authorize(Roles = "Admin,Funcionario")]
        public async Task<IActionResult> Create()
        {
            await LoadViewBags();
            return View();
        }

        // Emite uma nova passagem - apenas Admin e Funcionario
        [HttpPost]
        [Authorize(Roles = "Admin,Funcionario")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(IssueTicketRequestDto request)
        {
            try
            {
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

        // Realiza check-in - User pode fazer apenas nas suas próprias passagens
        public async Task<IActionResult> Checkin(int id)
        {
            try
            {
                var ticket = await _context.Tickets
                    .Include(t => t.Flight)
                    .FirstOrDefaultAsync(t => t.TicketId == id);

                if (ticket == null)
                {
                    TempData["Erro"] = "Passagem não encontrada";
                    return RedirectToAction(nameof(Index));
                }

                // Verifica permissão para usuário comum
                if (!User.IsInRole("Admin") && !User.IsInRole("Funcionario"))
                {
                    var userEmail = User.Identity.Name;
                    var customer = await _context.Customers.FirstOrDefaultAsync(c => c.Email == userEmail);
                    if (customer == null || ticket.CustomerId != customer.CustomerId)
                    {
                        TempData["Erro"] = "Você não tem permissão para fazer check-in desta passagem.";
                        return RedirectToAction(nameof(Index));
                    }
                }

                // Valida se o status permite check-in
                if (ticket.Status != TicketStatus.Confirmed)
                {
                    TempData["Erro"] = $"Check-in não permitido. Status atual: {ticket.Status}";
                    return RedirectToAction(nameof(Details), new { id = id });
                }

                // Impede check-in de voo já partido
                if (ticket.Flight != null && ticket.Flight.DepartureTime < DateTime.Now)
                {
                    TempData["Erro"] = "Não é possível fazer check-in de um voo que já partiu.";
                    return RedirectToAction(nameof(Details), new { id = id });
                }

                var result = await _ticketFacade.CheckinAsync(new CheckinRequestDto { TicketId = id });

                if (result.Success)
                    TempData["Sucesso"] = result.Message;
                else
                    TempData["Erro"] = result.ErrorMessage;

                return RedirectToAction(nameof(Details), new { id = id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao realizar check-in da passagem {TicketId}", id);
                TempData["Erro"] = "Erro ao realizar check-in";
                return RedirectToAction(nameof(Index));
            }
        }

        // Registra embarque - User pode fazer apenas nas suas próprias passagens
        public async Task<IActionResult> Boarding(int id)
        {
            try
            {
                var ticket = await _context.Tickets
                    .Include(t => t.Flight)
                    .FirstOrDefaultAsync(t => t.TicketId == id);

                if (ticket == null)
                {
                    TempData["Erro"] = "Passagem não encontrada";
                    return RedirectToAction(nameof(Index));
                }

                // Verifica permissão para usuário comum
                if (!User.IsInRole("Admin") && !User.IsInRole("Funcionario"))
                {
                    var userEmail = User.Identity.Name;
                    var customer = await _context.Customers.FirstOrDefaultAsync(c => c.Email == userEmail);
                    if (customer == null || ticket.CustomerId != customer.CustomerId)
                    {
                        TempData["Erro"] = "Você não tem permissão para registrar embarque desta passagem.";
                        return RedirectToAction(nameof(Index));
                    }
                }

                // Valida se já fez check-in
                if (ticket.Status != TicketStatus.CheckIn)
                {
                    TempData["Erro"] = $"Embarque não permitido. Status atual: {ticket.Status}. É necessário fazer check-in primeiro.";
                    return RedirectToAction(nameof(Details), new { id = id });
                }

                var result = await _ticketFacade.RegisterBoardingAsync(id);

                if (result.Success)
                    TempData["Sucesso"] = result.Message;
                else
                    TempData["Erro"] = result.ErrorMessage;

                return RedirectToAction(nameof(Details), new { id = id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao registrar embarque da passagem {TicketId}", id);
                TempData["Erro"] = "Erro ao registrar embarque";
                return RedirectToAction(nameof(Index));
            }
        }

        // Cancela uma passagem - User pode cancelar apenas suas próprias passagens
        public async Task<IActionResult> Cancel(int id)
        {
            try
            {
                var ticket = await _context.Tickets
                    .Include(t => t.Flight)
                    .FirstOrDefaultAsync(t => t.TicketId == id);

                if (ticket == null)
                {
                    TempData["Erro"] = "Passagem não encontrada";
                    return RedirectToAction(nameof(Index));
                }

                // Verifica permissão para usuário comum
                if (!User.IsInRole("Admin") && !User.IsInRole("Funcionario"))
                {
                    var userEmail = User.Identity.Name;
                    var customer = await _context.Customers.FirstOrDefaultAsync(c => c.Email == userEmail);
                    if (customer == null || ticket.CustomerId != customer.CustomerId)
                    {
                        TempData["Erro"] = "Você não tem permissão para cancelar esta passagem.";
                        return RedirectToAction(nameof(Index));
                    }
                }

                // Impede cancelamento de passagem já cancelada
                if (ticket.Status == TicketStatus.Cancelled)
                {
                    TempData["Info"] = "Esta passagem já está cancelada.";
                    return RedirectToAction(nameof(Details), new { id = id });
                }

                // Impede cancelamento de passagem já embarcada
                if (ticket.Status == TicketStatus.Boarded)
                {
                    TempData["Erro"] = "Não é possível cancelar uma passagem já embarcada.";
                    return RedirectToAction(nameof(Details), new { id = id });
                }

                // Impede cancelamento de voo já partido
                if (ticket.Flight != null && ticket.Flight.DepartureTime < DateTime.Now)
                {
                    TempData["Erro"] = "Não é possível cancelar uma passagem de um voo que já partiu.";
                    return RedirectToAction(nameof(Details), new { id = id });
                }

                var result = await _ticketFacade.CancelTicketAsync(new CancelTicketRequestDto { TicketId = id });

                if (result.Success)
                    TempData["Sucesso"] = result.Message;
                else
                    TempData["Erro"] = result.ErrorMessage;

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao cancelar passagem {TicketId}", id);
                TempData["Erro"] = "Erro ao cancelar passagem";
                return RedirectToAction(nameof(Index));
            }
        }

        // Passagens por cliente
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

                // Verifica permissão para usuário comum
                if (!User.IsInRole("Admin") && !User.IsInRole("Funcionario"))
                {
                    var userEmail = User.Identity.Name;
                    var currentCustomer = await _context.Customers.FirstOrDefaultAsync(c => c.Email == userEmail);
                    if (currentCustomer == null || currentCustomer.CustomerId != id)
                    {
                        TempData["Erro"] = "Você não tem permissão para ver passagens de outros clientes.";
                        return RedirectToAction(nameof(Index));
                    }
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

        // Passagens por voo - apenas Admin e Funcionario
        [Authorize(Roles = "Admin,Funcionario")]
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

        // Busca poltronas disponíveis via AJAX
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

        // Carrega os dados para os dropdowns da view
        private async Task LoadViewBags()
        {
            // Clientes ativos para o dropdown
            var activeCustomers = await _context.Customers
                .Where(c => c.IsActive)
                .OrderBy(c => c.Name)
                .ToListAsync();

            ViewBag.Customers = new SelectList(activeCustomers, "CustomerId", "Name");

            // Voos disponíveis para o dropdown
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