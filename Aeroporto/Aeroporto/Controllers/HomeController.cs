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
        /// Cria dados de teste no sistema (apenas para Admin)
        /// </summary>
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> SeedData()
        {
            try
            {
                // Verificar se já existem dados
                if (await _context.Flights.AnyAsync())
                {
                    TempData["Info"] = "O banco de dados já possui dados. Para criar novos dados, limpe o banco primeiro.";
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
                        new Customer { Name = "João Silva", Email = "joao.silva@email.com", Phone = "(11) 99999-9999", CPF = "123.456.789-00", City = "São Paulo", State = "SP", IsActive = true, RegistrationDate = DateTime.Now },
                        new Customer { Name = "Maria Santos", Email = "maria.santos@email.com", Phone = "(21) 98888-8888", CPF = "987.654.321-00", City = "Rio de Janeiro", State = "RJ", IsActive = true, RegistrationDate = DateTime.Now },
                        new Customer { Name = "Pedro Oliveira", Email = "pedro.oliveira@email.com", Phone = "(31) 97777-7777", CPF = "456.789.123-00", City = "Belo Horizonte", State = "MG", IsActive = true, RegistrationDate = DateTime.Now },
                        new Customer { Name = "Ana Costa", Email = "ana.costa@email.com", Phone = "(61) 96666-6666", CPF = "789.123.456-00", City = "Brasília", State = "DF", IsActive = true, RegistrationDate = DateTime.Now },
                        new Customer { Name = "Carlos Pereira", Email = "carlos.pereira@email.com", Phone = "(85) 95555-5555", CPF = "321.654.987-00", City = "Fortaleza", State = "CE", IsActive = true, RegistrationDate = DateTime.Now }
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
                    var cnf = await _context.Airports.FirstAsync(a => a.IATACode == "CNF");
                    var ssa = await _context.Airports.FirstAsync(a => a.IATACode == "SSA");
                    var aircraft737 = await _context.Aircrafts.FirstAsync(a => a.AircraftType.Contains("737"));
                    var aircraft320 = await _context.Aircrafts.FirstAsync(a => a.AircraftType.Contains("320"));
                    var aircraft195 = await _context.Aircrafts.FirstAsync(a => a.AircraftType.Contains("195"));

                    var flights = new[]
                    {
                        new Flight
                        {
                            FlightNumber = "LA1234",
                            DepartureAirportId = gru.AirportId,
                            ArrivalAirportId = sdu.AirportId,
                            AircraftId = aircraft737.AircraftId,
                            DepartureTime = DateTime.Now.AddDays(1).AddHours(8),
                            EstimatedArrivalTime = DateTime.Now.AddDays(1).AddHours(10)
                        },
                        new Flight
                        {
                            FlightNumber = "LA5678",
                            DepartureAirportId = gru.AirportId,
                            ArrivalAirportId = bsb.AirportId,
                            AircraftId = aircraft320.AircraftId,
                            DepartureTime = DateTime.Now.AddDays(2).AddHours(14),
                            EstimatedArrivalTime = DateTime.Now.AddDays(2).AddHours(17)
                        },
                        new Flight
                        {
                            FlightNumber = "LA9012",
                            DepartureAirportId = sdu.AirportId,
                            ArrivalAirportId = cnf.AirportId,
                            AircraftId = aircraft195.AircraftId,
                            DepartureTime = DateTime.Now.AddDays(3).AddHours(10),
                            EstimatedArrivalTime = DateTime.Now.AddDays(3).AddHours(13)
                        },
                        new Flight
                        {
                            FlightNumber = "LA3456",
                            DepartureAirportId = bsb.AirportId,
                            ArrivalAirportId = ssa.AirportId,
                            AircraftId = aircraft737.AircraftId,
                            DepartureTime = DateTime.Now.AddDays(4).AddHours(6),
                            EstimatedArrivalTime = DateTime.Now.AddDays(4).AddHours(9)
                        },
                        new Flight
                        {
                            FlightNumber = "LA7890",
                            DepartureAirportId = ssa.AirportId,
                            ArrivalAirportId = gru.AirportId,
                            AircraftId = aircraft320.AircraftId,
                            DepartureTime = DateTime.Now.AddDays(5).AddHours(20),
                            EstimatedArrivalTime = DateTime.Now.AddDays(6).AddHours(1)
                        }
                    };
                    _context.Flights.AddRange(flights);
                    await _context.SaveChangesAsync();
                    _logger.LogInformation("Voos criados com sucesso");

                    // 5. Criar poltronas para cada voo
                    foreach (var flight in flights)
                    {
                        var aircraft = await _context.Aircrafts.FindAsync(flight.AircraftId);
                        await CreateSeatsForFlight(flight.FlightId, aircraft.NumberOfSeats);
                    }
                    _logger.LogInformation("Poltronas criadas com sucesso");
                }

                // 6. Criar algumas passagens de exemplo
                if (!await _context.Tickets.AnyAsync())
                {
                    var customers = await _context.Customers.Take(3).ToListAsync();
                    var flights = await _context.Flights.Take(3).ToListAsync();
                    var random = new Random();

                    foreach (var flight in flights)
                    {
                        var availableSeats = await _context.Seats
                            .Where(s => s.FlightId == flight.FlightId && s.IsAvailable)
                            .Take(2)
                            .ToListAsync();

                        for (int i = 0; i < availableSeats.Count && i < customers.Count; i++)
                        {
                            var seat = availableSeats[i];
                            var customer = customers[i];

                            var ticket = new Ticket
                            {
                                FlightId = flight.FlightId,
                                CustomerId = customer.CustomerId,
                                SeatId = seat.SeatId,
                                TicketNumber = Guid.NewGuid().ToString("N").Substring(0, 16).ToUpper(),
                                IssueDate = DateTime.Now.AddDays(-random.Next(1, 10)),
                                Price = seat.Price,
                                Status = TicketStatus.Confirmed,
                                Class = seat.Class
                            };

                            _context.Tickets.Add(ticket);
                            seat.IsAvailable = false;
                        }
                    }
                    await _context.SaveChangesAsync();
                    _logger.LogInformation("Passagens criadas com sucesso");
                }

                await transaction.CommitAsync();

                TempData["Sucesso"] = "Dados de teste criados com sucesso! Voos, aeronaves, aeroportos, clientes e passagens foram adicionados ao sistema.";
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
                    seatClass = "Primeira";
                else if (i <= numberOfSeats * 0.2)
                    seatClass = "Executiva";
                else
                    seatClass = "Econômica";

                string location = position switch
                {
                    1 or 6 => "Janela",
                    2 or 5 => "Meio",
                    3 or 4 => "Corredor",
                    _ => "Corredor"
                };

                decimal price = seatClass switch
                {
                    "Primeira" => 800.00m,
                    "Executiva" => 500.00m,
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
    }
}