using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaAereo.Data.Context;
using SistemaAereo.Models.Entities;
using SistemaAereo.Models.Enums;
using SistemaAereo.Models.ViewModels;
using System.Diagnostics;

namespace SistemaAereo.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly AirportsContext _context;
        private readonly ILogger<HomeController> _logger;

        public HomeController(
            AirportsContext context,
            ILogger<HomeController> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Dashboard principal do sistema
        /// </summary>
        public async Task<IActionResult> Index()
        {
            try
            {
                var model = new DashboardViewModel();

                // =============================================
                // ESTATÍSTICAS PRINCIPAIS
                // =============================================

                model.TotalFlights = await _context.Flights.CountAsync();
                model.TotalCustomers = await _context.Customers.CountAsync(c => c.IsActive);
                model.TotalAircrafts = await _context.Aircrafts.CountAsync();
                model.TotalAirports = await _context.Airports.CountAsync();
                model.TotalTickets = await _context.Tickets.CountAsync();

                // =============================================
                // ESTATÍSTICAS DE PASSAGENS POR STATUS
                // =============================================

                model.ConfirmedTickets = await _context.Tickets
                    .CountAsync(t => t.Status == TicketStatus.Confirmed);

                model.CheckInTickets = await _context.Tickets
                    .CountAsync(t => t.Status == TicketStatus.CheckIn);

                model.BoardedTickets = await _context.Tickets
                    .CountAsync(t => t.Status == TicketStatus.Boarded);

                model.CancelledTickets = await _context.Tickets
                    .CountAsync(t => t.Status == TicketStatus.Cancelled);

                // =============================================
                // DADOS FINANCEIROS
                // =============================================

                model.TotalRevenue = await _context.Tickets
                    .Where(t => t.Status != TicketStatus.Cancelled)
                    .SumAsync(t => t.Price);

                var startOfMonth = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
                var endOfMonth = startOfMonth.AddMonths(1).AddDays(-1);

                model.CurrentMonthRevenue = await _context.Tickets
                    .Where(t => t.IssueDate >= startOfMonth &&
                                t.IssueDate <= endOfMonth &&
                                t.Status != TicketStatus.Cancelled)
                    .SumAsync(t => t.Price);

                // =============================================
                // PRÓXIMOS VOOS
                // =============================================

                model.UpcomingFlights = await _context.Flights
                    .AsNoTracking()
                    .Include(f => f.DepartureAirport)
                    .Include(f => f.ArrivalAirport)
                    .Include(f => f.Aircraft)
                    .Where(f => f.DepartureTime > DateTime.Now)
                    .OrderBy(f => f.DepartureTime)
                    .Take(5)
                    .ToListAsync();

                // =============================================
                // PASSAGENS RECENTES
                // =============================================

                model.RecentTickets = await _context.Tickets
                    .AsNoTracking()
                    .Include(t => t.Customer)
                    .Include(t => t.Flight)
                        .ThenInclude(f => f.DepartureAirport)
                    .Include(t => t.Flight)
                        .ThenInclude(f => f.ArrivalAirport)
                    .Include(t => t.Seat)
                    .OrderByDescending(t => t.IssueDate)
                    .Take(5)
                    .ToListAsync();

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao carregar dashboard");
                TempData["Erro"] = "Erro ao carregar dados do dashboard. Tente novamente mais tarde.";
                return View(new DashboardViewModel());
            }
        }

        /// <summary>
        /// Página de privacidade
        /// </summary>
        public IActionResult Privacy()
        {
            return View();
        }

        /// <summary>
        /// Página de erro genérica
        /// </summary>
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        /// <summary>
        /// Método temporário para popular o banco com dados de teste
        /// Acesse: /Home/SeedData
        /// </summary>
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> SeedData()
        {
            try
            {
                // Verificar se já existem dados
                if (await _context.Flights.AnyAsync())
                {
                    TempData["Info"] = "O banco de dados já possui dados.";
                    return RedirectToAction(nameof(Index));
                }

                using var transaction = await _context.Database.BeginTransactionAsync();

                // 1. Criar aeroportos
                if (!await _context.Airports.AnyAsync())
                {
                    var airports = new[]
                    {
                        new Airport { Name = "Aeroporto Internacional de São Paulo/Guarulhos", IATACode = "GRU", City = "São Paulo", Country = "Brasil" },
                        new Airport { Name = "Aeroporto Santos Dumont", IATACode = "SDU", City = "Rio de Janeiro", Country = "Brasil" },
                        new Airport { Name = "Aeroporto Internacional de Brasília", IATACode = "BSB", City = "Brasília", Country = "Brasil" },
                        new Airport { Name = "Aeroporto Internacional de Confins", IATACode = "CNF", City = "Belo Horizonte", Country = "Brasil" },
                        new Airport { Name = "Aeroporto Internacional de Salvador", IATACode = "SSA", City = "Salvador", Country = "Brasil" }
                    };
                    _context.Airports.AddRange(airports);
                    await _context.SaveChangesAsync();
                    _logger.LogInformation("Aeroportos criados com sucesso");
                }

                // 2. Criar aeronaves
                if (!await _context.Aircrafts.AnyAsync())
                {
                    var aircrafts = new[]
                    {
                        new Aircraft { AircraftType = "Boeing 737-800", NumberOfSeats = 180 },
                        new Aircraft { AircraftType = "Airbus A320", NumberOfSeats = 150 },
                        new Aircraft { AircraftType = "Embraer E195", NumberOfSeats = 120 }
                    };
                    _context.Aircrafts.AddRange(aircrafts);
                    await _context.SaveChangesAsync();
                    _logger.LogInformation("Aeronaves criadas com sucesso");
                }

                // 3. Criar clientes
                if (!await _context.Customers.AnyAsync())
                {
                    var customers = new[]
                    {
                        new Customer { Name = "João Silva", Email = "joao.silva@email.com", Phone = "(11) 99999-9999", CPF = "123.456.789-00", City = "São Paulo", State = "SP", IsActive = true },
                        new Customer { Name = "Maria Santos", Email = "maria.santos@email.com", Phone = "(21) 98888-8888", CPF = "987.654.321-00", City = "Rio de Janeiro", State = "RJ", IsActive = true },
                        new Customer { Name = "Pedro Oliveira", Email = "pedro.oliveira@email.com", Phone = "(31) 97777-7777", CPF = "456.789.123-00", City = "Belo Horizonte", State = "MG", IsActive = true },
                        new Customer { Name = "Ana Costa", Email = "ana.costa@email.com", Phone = "(61) 96666-6666", CPF = "789.123.456-00", City = "Brasília", State = "DF", IsActive = true }
                    };
                    _context.Customers.AddRange(customers);
                    await _context.SaveChangesAsync();
                    _logger.LogInformation("Clientes criados com sucesso");
                }

                // 4. Criar voos
                if (!await _context.Flights.AnyAsync())
                {
                    var gru = await _context.Airports.FirstAsync(a => a.IATACode == "GRU");
                    var sdu = await _context.Airports.FirstAsync(a => a.IATACode == "SDU");
                    var bsb = await _context.Airports.FirstAsync(a => a.IATACode == "BSB");
                    var aircraft = await _context.Aircrafts.FirstAsync();

                    var flights = new[]
                    {
                        new Flight
                        {
                            FlightNumber = "LA1234",
                            DepartureAirportId = gru.AirportId,
                            ArrivalAirportId = sdu.AirportId,
                            AircraftId = aircraft.AircraftId,
                            DepartureTime = DateTime.Now.AddDays(1).AddHours(8),
                            EstimatedArrivalTime = DateTime.Now.AddDays(1).AddHours(10)
                        },
                        new Flight
                        {
                            FlightNumber = "LA5678",
                            DepartureAirportId = gru.AirportId,
                            ArrivalAirportId = bsb.AirportId,
                            AircraftId = aircraft.AircraftId,
                            DepartureTime = DateTime.Now.AddDays(2).AddHours(14),
                            EstimatedArrivalTime = DateTime.Now.AddDays(2).AddHours(17)
                        },
                        new Flight
                        {
                            FlightNumber = "LA9012",
                            DepartureAirportId = sdu.AirportId,
                            ArrivalAirportId = bsb.AirportId,
                            AircraftId = aircraft.AircraftId,
                            DepartureTime = DateTime.Now.AddDays(3).AddHours(10),
                            EstimatedArrivalTime = DateTime.Now.AddDays(3).AddHours(13)
                        }
                    };
                    _context.Flights.AddRange(flights);
                    await _context.SaveChangesAsync();
                    _logger.LogInformation("Voos criados com sucesso");

                    // 5. Criar poltronas para cada voo
                    foreach (var flight in flights)
                    {
                        await CreateSeatsForFlight(flight.FlightId, aircraft.NumberOfSeats);
                    }
                    _logger.LogInformation("Poltronas criadas com sucesso");
                }

                await transaction.CommitAsync();

                TempData["Sucesso"] = "Dados de teste criados com sucesso!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao criar dados de teste");
                TempData["Erro"] = $"Erro ao criar dados: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }

        /// <summary>
        /// Método auxiliar para criar poltronas para um voo
        /// </summary>
        private async Task CreateSeatsForFlight(int flightId, int numberOfSeats)
        {
            var seats = new List<Seat>();
            var random = new Random();

            for (int i = 1; i <= numberOfSeats; i++)
            {
                var row = (i - 1) / 6 + 1;
                var position = (i - 1) % 6 + 1;
                var letter = ((char)('A' + (position - 1))).ToString();

                string seatClass;
                if (i <= numberOfSeats * 0.05)
                    seatClass = SeatClass.FirstClass;
                else if (i <= numberOfSeats * 0.2)
                    seatClass = SeatClass.Executive;
                else
                    seatClass = SeatClass.Economy;

                string location = position switch
                {
                    1 or 6 => SeatLocation.Window,
                    2 or 5 => SeatLocation.Middle,
                    3 or 4 => SeatLocation.Aisle,
                    _ => SeatLocation.Aisle
                };

                decimal price = seatClass switch
                {
                    SeatClass.FirstClass => 800.00m,
                    SeatClass.Executive => 500.00m,
                    _ => 300.00m
                };

                price += random.Next(-50, 51);

                var seat = new Seat
                {
                    FlightId = flightId,
                    SeatNumber = $"{row}{letter}",
                    IsAvailable = true,
                    Location = location,
                    Class = seatClass,
                    Price = price
                };

                seats.Add(seat);
            }

            await _context.Seats.AddRangeAsync(seats);
            await _context.SaveChangesAsync();
        }
    }
}