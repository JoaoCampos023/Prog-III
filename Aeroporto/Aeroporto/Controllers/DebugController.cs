using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaAereo.Data.Context;
using SistemaAereo.Models.Entities;
using SistemaAereo.Services.Interfaces;

namespace SistemaAereo.Controllers
{
    public class DebugController : Controller
    {
        private readonly AirportsContext _context;
        private readonly ISeatService _seatService;
        private readonly ILogger<DebugController> _logger;

        public DebugController(
            AirportsContext context,
            ISeatService seatService,
            ILogger<DebugController> logger)
        {
            _context = context;
            _seatService = seatService;
            _logger = logger;
        }

        // =============================================
        // MÉTODOS DE VISUALIZAÇÃO DE DADOS
        // =============================================

        /// <summary>
        /// Exibe todos os dados do sistema em uma view
        /// </summary>
        public async Task<IActionResult> Dados()
        {
            var dados = new
            {
                Flights = await _context.Flights
                    .AsNoTracking()
                    .Include(f => f.DepartureAirport)
                    .Include(f => f.ArrivalAirport)
                    .Include(f => f.Aircraft)
                    .Include(f => f.Seats)
                    .Select(f => new
                    {
                        f.FlightId,
                        f.FlightNumber,
                        Origin = f.DepartureAirport.IATACode,
                        Destination = f.ArrivalAirport.IATACode,
                        f.DepartureTime,
                        IsFuture = f.DepartureTime > DateTime.Now,
                        TotalSeats = f.Seats.Count,
                        AvailableSeats = f.Seats.Count(s => s.IsAvailable)
                    })
                    .ToListAsync(),

                Customers = await _context.Customers
                    .AsNoTracking()
                    .Where(c => c.IsActive)
                    .Select(c => new { c.CustomerId, c.Name })
                    .ToListAsync(),

                Aircrafts = await _context.Aircrafts
                    .AsNoTracking()
                    .Select(a => new { a.AircraftId, a.AircraftType })
                    .ToListAsync(),

                Airports = await _context.Airports
                    .AsNoTracking()
                    .Select(a => new { a.AirportId, a.Name, a.IATACode })
                    .ToListAsync()
            };

            return View(dados);
        }

        /// <summary>
        /// Retorna dados em formato JSON para debug
        /// </summary>
        public async Task<JsonResult> ApiDados()
        {
            var dados = new
            {
                Flights = await _context.Flights
                    .AsNoTracking()
                    .Include(f => f.DepartureAirport)
                    .Include(f => f.ArrivalAirport)
                    .Include(f => f.Seats)
                    .Select(f => new
                    {
                        f.FlightId,
                        f.FlightNumber,
                        Origin = f.DepartureAirport.IATACode,
                        Destination = f.ArrivalAirport.IATACode,
                        f.DepartureTime,
                        IsFuture = f.DepartureTime > DateTime.Now,
                        TotalSeats = f.Seats.Count,
                        AvailableSeats = f.Seats.Count(s => s.IsAvailable)
                    })
                    .ToListAsync(),

                ActiveCustomers = await _context.Customers
                    .AsNoTracking()
                    .CountAsync(c => c.IsActive)
            };

            return Json(dados);
        }

        // =============================================
        // MÉTODOS DE CRIAÇÃO DE DADOS TESTE
        // =============================================

        /// <summary>
        /// Cria dados de teste para o sistema
        /// </summary>
        [HttpPost]
        public async Task<JsonResult> CriarDadosTeste()
        {
            try
            {
                var flightsExist = await _context.Flights.AnyAsync();
                var customersExist = await _context.Customers.AnyAsync();

                if (flightsExist || customersExist)
                {
                    return Json(new
                    {
                        success = false,
                        message = "Já existem dados no sistema. Use os dados existentes ou limpe o banco primeiro."
                    });
                }

                using var transaction = await _context.Database.BeginTransactionAsync();

                try
                {
                    // Criar aeronave
                    var aircraft = new Aircraft
                    {
                        AircraftType = "Boeing 737-800",
                        NumberOfSeats = 180
                    };
                    _context.Aircrafts.Add(aircraft);

                    // Criar aeroportos
                    var airports = new[]
                    {
                        new Airport { Name = "Aeroporto Internacional de São Paulo/Guarulhos", IATACode = "GRU", City = "São Paulo", Country = "Brasil" },
                        new Airport { Name = "Aeroporto Santos Dumont", IATACode = "SDU", City = "Rio de Janeiro", Country = "Brasil" },
                        new Airport { Name = "Aeroporto Internacional de Brasília", IATACode = "BSB", City = "Brasília", Country = "Brasil" }
                    };
                    _context.Airports.AddRange(airports);

                    await _context.SaveChangesAsync();

                    // Criar voos
                    var flights = new[]
                    {
                        new Flight
                        {
                            FlightNumber = "LA1234",
                            DepartureAirportId = airports[0].AirportId,
                            ArrivalAirportId = airports[1].AirportId,
                            AircraftId = aircraft.AircraftId,
                            DepartureTime = DateTime.Now.AddDays(1).AddHours(2),
                            EstimatedArrivalTime = DateTime.Now.AddDays(1).AddHours(4)
                        },
                        new Flight
                        {
                            FlightNumber = "LA5678",
                            DepartureAirportId = airports[0].AirportId,
                            ArrivalAirportId = airports[2].AirportId,
                            AircraftId = aircraft.AircraftId,
                            DepartureTime = DateTime.Now.AddDays(2).AddHours(3),
                            EstimatedArrivalTime = DateTime.Now.AddDays(2).AddHours(5)
                        }
                    };
                    _context.Flights.AddRange(flights);

                    // Criar clientes
                    var customers = new[]
                    {
                        new Customer
                        {
                            Name = "João Silva",
                            Email = "joao.silva@email.com",
                            Phone = "(11) 99999-9999",
                            CPF = "123.456.789-00",
                            City = "São Paulo",
                            State = "SP",
                            IsActive = true
                        },
                        new Customer
                        {
                            Name = "Maria Santos",
                            Email = "maria.santos@email.com",
                            Phone = "(21) 98888-8888",
                            CPF = "987.654.321-00",
                            City = "Rio de Janeiro",
                            State = "RJ",
                            IsActive = true
                        }
                    };
                    _context.Customers.AddRange(customers);

                    await _context.SaveChangesAsync();

                    // Criar poltronas para cada voo
                    foreach (var flight in flights)
                    {
                        await _seatService.CreateSeatsForFlightAsync(flight.FlightId, aircraft.NumberOfSeats);
                    }

                    await transaction.CommitAsync();

                    return Json(new
                    {
                        success = true,
                        message = "Dados de teste criados com sucesso! Foram criados 2 voos, 2 clientes e poltronas."
                    });
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao criar dados de teste");
                return Json(new { success = false, message = $"Erro ao criar dados de teste: {ex.Message}" });
            }
        }
    }
}