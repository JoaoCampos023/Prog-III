using Microsoft.EntityFrameworkCore;
using SistemaAereo.Data.Context;
using SistemaAereo.Models.Entities;
using SistemaAereo.Models.Enums;
using SistemaAereo.Services.Interfaces;

namespace SistemaAereo.Services
{
    // Implementação do serviço de gerenciamento de poltronas
    public class SeatService : ISeatService
    {
        private readonly AirportsContext _context;
        private readonly ILogger<SeatService> _logger;

        public SeatService(AirportsContext context, ILogger<SeatService> logger)
        {
            _context = context;
            _logger = logger;
        }

        // Cria as poltronas para um voo específico
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

                // Verifica se o voo já possui poltronas (evita duplicação)
                var existingSeats = await _context.Seats.AnyAsync(s => s.FlightId == flightId);
                if (existingSeats)
                {
                    _logger.LogInformation($"Voo {flightId} já possui poltronas cadastradas");
                    return await _context.Seats.Where(s => s.FlightId == flightId).ToListAsync();
                }

                // Define o número total de poltronas (usa o da aeronave ou fallback de 50)
                int totalSeats = numberOfSeats ?? flight.Aircraft?.NumberOfSeats ?? 50;
                var seats = new List<Seat>();
                var random = new Random();

                for (int i = 1; i <= totalSeats; i++)
                {
                    // Calcula fileira e letra do assento (formato: 1A, 1B, 2A, etc.)
                    var row = (i - 1) / 6 + 1;
                    var position = (i - 1) % 6 + 1;
                    var letter = ((char)('A' + (position - 1))).ToString();

                    // Define a classe da poltrona baseada na posição
                    string seatClass;
                    if (i <= totalSeats * 0.05)
                        seatClass = SeatClass.FirstClass;      // 5% primeiras poltronas
                    else if (i <= totalSeats * 0.2)
                        seatClass = SeatClass.Executive;       // 15% seguintes
                    else
                        seatClass = SeatClass.Economy;         // Restante

                    // Define a localização na fileira
                    string location = position switch
                    {
                        1 or 6 => SeatLocation.Window,   // Janela
                        2 or 5 => SeatLocation.Middle,   // Meio
                        3 or 4 => SeatLocation.Aisle,    // Corredor
                        _ => SeatLocation.Aisle
                    };

                    // Define o preço baseado na classe
                    decimal price = seatClass switch
                    {
                        SeatClass.FirstClass => 800.00m,
                        SeatClass.Executive => 500.00m,
                        _ => 300.00m
                    };

                    // Adiciona uma pequena variação aleatória ao preço
                    price += random.Next(-50, 51);

                    var seat = new Seat
                    {
                        FlightId = flightId,
                        SeatNumber = $"{row}{letter}",
                        IsAvailable = true,
                        Location = location,
                        Class = seatClass,
                        Price = Math.Max(price, 50) // Garante preço mínimo de R$ 50
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

        // Verifica se o voo já possui poltronas
        public async Task<bool> HasSeatsAsync(int flightId)
        {
            return await _context.Seats.AnyAsync(s => s.FlightId == flightId);
        }

        // Obtém o total de poltronas disponíveis em um voo
        public async Task<int> GetTotalAvailableSeatsAsync(int flightId)
        {
            return await _context.Seats
                .AsNoTracking()
                .CountAsync(s => s.FlightId == flightId && s.IsAvailable);
        }

        // Obtém o total de poltronas ocupadas em um voo
        public async Task<int> GetTotalOccupiedSeatsAsync(int flightId)
        {
            return await _context.Seats
                .AsNoTracking()
                .CountAsync(s => s.FlightId == flightId && !s.IsAvailable);
        }
    }
}