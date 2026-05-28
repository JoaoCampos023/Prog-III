using Microsoft.EntityFrameworkCore;
using SistemaAereo.Data.Context;
using SistemaAereo.Models.Entities;
using SistemaAereo.Models.Enums;
using SistemaAereo.Services.Interfaces;

namespace SistemaAereo.Services
{
    public class SeatService : ISeatService
    {
        private readonly AirportsContext _context;
        private readonly ILogger<SeatService> _logger;

        public SeatService(AirportsContext context, ILogger<SeatService> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Cria as poltronas para um voo específico
        /// </summary>
        public async Task<List<Seat>> CreateSeatsForFlightAsync(int flightId, int? numberOfSeats = null)
        {
            try
            {
                var flight = await _context.Flights
                    .Include(f => f.Aircraft)
                    .FirstOrDefaultAsync(f => f.FlightId == flightId);

                if (flight == null)
                {
                    _logger.LogWarning($"Voo {flightId} não encontrado para criação de poltronas");
                    return new List<Seat>();
                }

                // Verificar se já existem poltronas
                var existingSeats = await _context.Seats.AnyAsync(s => s.FlightId == flightId);
                if (existingSeats)
                {
                    _logger.LogInformation($"Voo {flightId} já possui poltronas cadastradas");
                    return await _context.Seats.Where(s => s.FlightId == flightId).ToListAsync();
                }

                int totalSeats = numberOfSeats ?? flight.Aircraft?.NumberOfSeats ?? 50;
                var seats = new List<Seat>();
                var random = new Random();

                for (int i = 1; i <= totalSeats; i++)
                {
                    var row = (i - 1) / 6 + 1;
                    var position = (i - 1) % 6 + 1;
                    var letter = ((char)('A' + (position - 1))).ToString();

                    // Definir tipo baseado na posição
                    string seatClass;
                    if (i <= totalSeats * 0.05)
                        seatClass = SeatClass.FirstClass;
                    else if (i <= totalSeats * 0.2)
                        seatClass = SeatClass.Executive;
                    else
                        seatClass = SeatClass.Economy;

                    // Definir localização baseada no assento
                    string location = position switch
                    {
                        1 or 6 => SeatLocation.Window,
                        2 or 5 => SeatLocation.Middle,
                        3 or 4 => SeatLocation.Aisle,
                        _ => SeatLocation.Aisle
                    };

                    // Definir preço baseado no tipo
                    decimal price = seatClass switch
                    {
                        SeatClass.FirstClass => 800.00m,
                        SeatClass.Executive => 500.00m,
                        _ => 300.00m
                    };

                    // Adicionar variação aleatória
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

                _logger.LogInformation($"Criadas {seats.Count} poltronas para o voo {flight.FlightNumber}");
                return seats;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Erro ao criar poltronas para voo {flightId}");
                throw;
            }
        }

        /// <summary>
        /// Verifica se o voo já possui poltronas
        /// </summary>
        public async Task<bool> HasSeatsAsync(int flightId)
        {
            return await _context.Seats.AnyAsync(s => s.FlightId == flightId);
        }

        /// <summary>
        /// Obtém o total de poltronas disponíveis em um voo
        /// </summary>
        public async Task<int> GetTotalAvailableSeatsAsync(int flightId)
        {
            return await _context.Seats
                .AsNoTracking()
                .CountAsync(s => s.FlightId == flightId && s.IsAvailable);
        }

        /// <summary>
        /// Obtém o total de poltronas ocupadas em um voo
        /// </summary>
        public async Task<int> GetTotalOccupiedSeatsAsync(int flightId)
        {
            return await _context.Seats
                .AsNoTracking()
                .CountAsync(s => s.FlightId == flightId && !s.IsAvailable);
        }
    }
}