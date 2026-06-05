using Microsoft.EntityFrameworkCore;
using SistemaAereo.Data.Context;
using SistemaAereo.Facades.Interfaces;
using SistemaAereo.Models.DTOs;
using SistemaAereo.Models.Entities;
using SistemaAereo.Models.Enums;
using SistemaAereo.Repositories.Interfaces;
using SistemaAereo.Services.Interfaces;

namespace SistemaAereo.Facades.Implementations
{
    // Implementação da fachada de voos
    // Centraliza toda a lógica de negócio relacionada a voos
    public class FlightFacade : IFlightFacade
    {
        // Dependências injetadas
        private readonly IFlightRepository _flightRepository;
        private readonly ISeatService _seatService;
        private readonly AirportsContext _context;
        private readonly ILogger<FlightFacade> _logger;

        public FlightFacade(
            IFlightRepository flightRepository,
            ISeatService seatService,
            AirportsContext context,
            ILogger<FlightFacade> logger)
        {
            _flightRepository = flightRepository;
            _seatService = seatService;
            _context = context;
            _logger = logger;
        }

        // Cria um novo voo com todas as dependências
        public async Task<FlightResultDto> CreateFlightAsync(Flight flight)
        {
            _logger.LogInformation($"Iniciando criação do voo - Número: {flight.FlightNumber}");

            // Validações
            if (flight.DepartureAirportId == flight.ArrivalAirportId)
                return FlightResultDto.Fail("O aeroporto de destino deve ser diferente do aeroporto de origem.");

            if (flight.EstimatedArrivalTime <= flight.DepartureTime)
                return FlightResultDto.Fail("O horário de chegada deve ser posterior ao horário de saída.");

            if (flight.DepartureTime < DateTime.Now)
                return FlightResultDto.Fail("Não é possível cadastrar um voo com data/hora no passado.");

            // Verifica se o número do voo já existe
            var flightNumberExists = await _flightRepository.FlightNumberExistsAsync(flight.FlightNumber);
            if (flightNumberExists)
                return FlightResultDto.Fail("Este número de voo já está cadastrado.");

            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                // Salvar voo
                _context.Flights.Add(flight);
                await _context.SaveChangesAsync();

                // Criar poltronas automaticamente
                await _seatService.CreateSeatsForFlightAsync(flight.FlightId);

                await transaction.CommitAsync();

                _logger.LogInformation($"Voo criado com sucesso - ID: {flight.FlightId}, Número: {flight.FlightNumber}");
                return FlightResultDto.Ok(flight, "Voo cadastrado com sucesso!");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, $"Erro ao criar voo {flight.FlightNumber}");
                return FlightResultDto.Fail($"Erro ao criar voo: {ex.Message}");
            }
        }

        // Atualiza um voo existente
        public async Task<FlightResultDto> UpdateFlightAsync(Flight flight)
        {
            _logger.LogInformation($"Iniciando atualização do voo - ID: {flight.FlightId}");

            // Valida se a data não é passada
            if (flight.DepartureTime < DateTime.Now)
                return FlightResultDto.Fail("Não é possível editar para uma data/hora no passado.");

            if (flight.DepartureAirportId == flight.ArrivalAirportId)
                return FlightResultDto.Fail("O aeroporto de destino deve ser diferente do aeroporto de origem.");

            if (flight.EstimatedArrivalTime <= flight.DepartureTime)
                return FlightResultDto.Fail("O horário de chegada deve ser posterior ao horário de saída.");

            // Verifica se o número do voo já existe (excluindo o próprio voo)
            var flightNumberExists = await _flightRepository.FlightNumberExistsAsync(flight.FlightNumber, flight.FlightId);
            if (flightNumberExists)
                return FlightResultDto.Fail("Este número de voo já está cadastrado.");

            try
            {
                _context.Flights.Update(flight);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Voo atualizado com sucesso - ID: {flight.FlightId}");
                return FlightResultDto.Ok(flight, "Voo atualizado com sucesso!");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Erro ao atualizar voo {flight.FlightId}");
                return FlightResultDto.Fail($"Erro ao atualizar voo: {ex.Message}");
            }
        }

        // Exclui um voo e todas suas dependências
        public async Task<FlightResultDto> DeleteFlightAsync(int flightId)
        {
            _logger.LogInformation($"Iniciando exclusão do voo - ID: {flightId}");

            var flight = await _context.Flights
                .Include(f => f.Seats)
                .Include(f => f.Tickets)
                .FirstOrDefaultAsync(f => f.FlightId == flightId);

            if (flight == null)
                return FlightResultDto.Fail("Voo não encontrado");

            // Verifica se existem passagens vendidas (não canceladas)
            var hasTickets = flight.Tickets.Any(t => t.Status != TicketStatus.Cancelled);
            if (hasTickets)
                return FlightResultDto.Fail("Não é possível excluir o voo pois existem passagens vendidas.");

            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                // Remove as poltronas primeiro
                if (flight.Seats.Any())
                    _context.Seats.RemoveRange(flight.Seats);

                // Remove as escalas
                var stopovers = await _context.Stopovers.Where(s => s.FlightId == flightId).ToListAsync();
                if (stopovers.Any())
                    _context.Stopovers.RemoveRange(stopovers);

                // Remove o voo
                _context.Flights.Remove(flight);
                await _context.SaveChangesAsync();

                await transaction.CommitAsync();

                _logger.LogInformation($"Voo excluído com sucesso - ID: {flightId}");
                return FlightResultDto.Ok(flight, "Voo excluído com sucesso!");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, $"Erro ao excluir voo {flightId}");
                return FlightResultDto.Fail($"Erro ao excluir voo: {ex.Message}");
            }
        }

        // Recria as poltronas de um voo
        public async Task<FlightResultDto> RecreateSeatsAsync(int flightId)
        {
            _logger.LogInformation($"Recriando poltronas do voo - ID: {flightId}");

            var flight = await _context.Flights
                .Include(f => f.Seats)
                .FirstOrDefaultAsync(f => f.FlightId == flightId);

            if (flight == null)
                return FlightResultDto.Fail("Voo não encontrado");

            // Verifica se existem passagens vendidas
            var hasTickets = await _context.Tickets
                .AnyAsync(t => t.FlightId == flightId && t.Status != TicketStatus.Cancelled);

            if (hasTickets)
                return FlightResultDto.Fail("Não é possível recriar poltronas pois existem passagens vendidas para este voo.");

            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                // Remove poltronas existentes
                if (flight.Seats.Any())
                {
                    _context.Seats.RemoveRange(flight.Seats);
                    await _context.SaveChangesAsync();
                }

                // Cria novas poltronas
                await _seatService.CreateSeatsForFlightAsync(flightId);

                await transaction.CommitAsync();

                _logger.LogInformation($"Poltronas recriadas com sucesso - Voo ID: {flightId}");
                return FlightResultDto.Ok(flight, "Poltronas recriadas com sucesso!");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, $"Erro ao recriar poltronas do voo {flightId}");
                return FlightResultDto.Fail($"Erro ao recriar poltronas: {ex.Message}");
            }
        }

        // Obtém estatísticas completas de um voo
        public async Task<FlightStatisticsDto> GetFlightStatisticsAsync(int flightId)
        {
            var flight = await _context.Flights
                .Include(f => f.Seats)
                .FirstOrDefaultAsync(f => f.FlightId == flightId);

            if (flight == null)
                return null;

            // Calcula estatísticas
            var totalSeats = flight.Seats.Count;
            var availableSeats = flight.Seats.Count(s => s.IsAvailable);
            var occupiedSeats = totalSeats - availableSeats;

            var totalTickets = await _context.Tickets
                .CountAsync(t => t.FlightId == flightId && t.Status != TicketStatus.Cancelled);

            var totalRevenue = await _context.Tickets
                .Where(t => t.FlightId == flightId && t.Status != TicketStatus.Cancelled)
                .SumAsync(t => t.Price);

            var occupancyPercentage = totalSeats > 0
                ? (double)occupiedSeats / totalSeats * 100
                : 0;

            return new FlightStatisticsDto
            {
                TotalSeats = totalSeats,
                AvailableSeats = availableSeats,
                OccupiedSeats = occupiedSeats,
                TotalTickets = totalTickets,
                TotalRevenue = totalRevenue,
                OccupancyPercentage = Math.Round(occupancyPercentage, 2)
            };
        }
    }
}