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
    public class TicketFacade : ITicketFacade
    {
        private readonly ITicketRepository _ticketRepository;
        private readonly ISeatRepository _seatRepository;
        private readonly ICustomerRepository _customerRepository;
        private readonly IFlightRepository _flightRepository;
        private readonly ISeatService _seatService;
        private readonly AirportsContext _context;
        private readonly ILogger<TicketFacade> _logger;

        public TicketFacade(
            ITicketRepository ticketRepository,
            ISeatRepository seatRepository,
            ICustomerRepository customerRepository,
            IFlightRepository flightRepository,
            ISeatService seatService,
            AirportsContext context,
            ILogger<TicketFacade> logger)
        {
            _ticketRepository = ticketRepository;
            _seatRepository = seatRepository;
            _customerRepository = customerRepository;
            _flightRepository = flightRepository;
            _seatService = seatService;
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Emite uma nova passagem
        /// </summary>
        public async Task<TicketResultDto> IssueTicketAsync(IssueTicketRequestDto request)
        {
            _logger.LogInformation($"Iniciando emissão de passagem - Cliente: {request.CustomerId}, Voo: {request.FlightId}, Poltrona: {request.SeatId}");

            // Validações iniciais
            var customer = await ValidateCustomerAsync(request.CustomerId);
            if (customer == null)
                return TicketResultDto.Fail("Cliente não encontrado ou inativo");

            var flight = await ValidateFlightAsync(request.FlightId);
            if (flight == null)
                return TicketResultDto.Fail("Voo não encontrado");

            if (flight.DepartureTime < DateTime.Now)
                return TicketResultDto.Fail("Não é possível emitir passagem para um voo que já partiu");

            // Iniciar transação
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                // Verificar e bloquear poltrona
                var seat = await ValidateSeatAsync(request.SeatId);
                if (seat == null)
                    return TicketResultDto.Fail("Poltrona não encontrada ou indisponível");

                // Verificar dupla ocupação
                if (await IsSeatOccupiedAsync(seat.SeatId))
                    return TicketResultDto.Fail("Poltrona já foi ocupada por outra passagem");

                // Marcar poltrona como indisponível
                seat.IsAvailable = false;
                _context.Seats.Update(seat);
                await _context.SaveChangesAsync();

                // Criar passagem
                var ticket = CreateTicket(request, seat);
                _context.Tickets.Add(ticket);
                await _context.SaveChangesAsync();

                await transaction.CommitAsync();

                _logger.LogInformation($"Passagem emitida com sucesso - Bilhete: {ticket.TicketNumber}");
                return TicketResultDto.Ok(ticket);
            }
            catch (DbUpdateConcurrencyException ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Conflito de concorrência ao emitir passagem");
                return TicketResultDto.Fail("A poltrona foi comprada por outro usuário. Tente novamente.");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Erro ao emitir passagem");
                return TicketResultDto.Fail($"Erro ao processar a compra: {ex.Message}");
            }
        }

        /// <summary>
        /// Cancela uma passagem existente
        /// </summary>
        public async Task<TicketResultDto> CancelTicketAsync(CancelTicketRequestDto request)
        {
            _logger.LogInformation($"Iniciando cancelamento de passagem - ID: {request.TicketId}");

            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var ticket = await _context.Tickets
                    .Include(t => t.Flight)
                    .FirstOrDefaultAsync(t => t.TicketId == request.TicketId);

                if (ticket == null)
                    return TicketResultDto.Fail("Passagem não encontrada");

                if (ticket.Status == TicketStatus.Cancelled)
                    return TicketResultDto.Fail("Passagem já está cancelada");

                if (ticket.Status == TicketStatus.Boarded)
                    return TicketResultDto.Fail("Não é possível cancelar uma passagem já embarcada");

                if (ticket.Flight != null && ticket.Flight.DepartureTime < DateTime.Now)
                    return TicketResultDto.Fail("Não é possível cancelar uma passagem de um voo que já partiu");

                // Atualizar status da passagem
                ticket.Status = TicketStatus.Cancelled;
                _context.Tickets.Update(ticket);
                await _context.SaveChangesAsync();

                // Liberar poltrona
                var seat = await _seatRepository.GetByIdAsync(ticket.SeatId);
                if (seat != null)
                {
                    seat.IsAvailable = true;
                    _context.Seats.Update(seat);
                    await _context.SaveChangesAsync();
                }

                await transaction.CommitAsync();

                _logger.LogInformation($"Passagem cancelada com sucesso - ID: {request.TicketId}");
                return TicketResultDto.CancelOk();
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, $"Erro ao cancelar passagem {request.TicketId}");
                return TicketResultDto.Fail($"Erro ao cancelar passagem: {ex.Message}");
            }
        }

        /// <summary>
        /// Realiza check-in de uma passagem
        /// </summary>
        public async Task<TicketResultDto> CheckinAsync(CheckinRequestDto request)
        {
            _logger.LogInformation($"Iniciando check-in - Passagem ID: {request.TicketId}");

            try
            {
                var ticket = await _ticketRepository.GetTicketCompleteAsync(request.TicketId);
                if (ticket == null)
                    return TicketResultDto.Fail("Passagem não encontrada");

                if (ticket.Status != TicketStatus.Confirmed)
                    return TicketResultDto.Fail($"Check-in não permitido. Status atual: {ticket.Status}");

                if (ticket.Flight != null && ticket.Flight.DepartureTime < DateTime.Now)
                    return TicketResultDto.Fail("Não é possível fazer check-in de um voo que já partiu");

                ticket.Status = TicketStatus.CheckIn;
                await _ticketRepository.UpdateAsync(ticket);

                _logger.LogInformation($"Check-in realizado - Passagem ID: {request.TicketId}");
                return TicketResultDto.CheckinOk();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Erro ao realizar check-in da passagem {request.TicketId}");
                return TicketResultDto.Fail($"Erro ao realizar check-in: {ex.Message}");
            }
        }

        /// <summary>
        /// Registra embarque de uma passagem
        /// </summary>
        public async Task<TicketResultDto> RegisterBoardingAsync(int ticketId)
        {
            _logger.LogInformation($"Registrando embarque - Passagem ID: {ticketId}");

            try
            {
                var ticket = await _ticketRepository.GetByIdAsync(ticketId);
                if (ticket == null)
                    return TicketResultDto.Fail("Passagem não encontrada");

                if (ticket.Status != TicketStatus.CheckIn)
                    return TicketResultDto.Fail($"Embarque não permitido. Status atual: {ticket.Status}. É necessário fazer check-in primeiro.");

                ticket.Status = TicketStatus.Boarded;
                await _ticketRepository.UpdateAsync(ticket);

                _logger.LogInformation($"Embarque registrado - Passagem ID: {ticketId}");
                return TicketResultDto.BoardingOk();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Erro ao registrar embarque da passagem {ticketId}");
                return TicketResultDto.Fail($"Erro ao registrar embarque: {ex.Message}");
            }
        }

        /// <summary>
        /// Obtém detalhes completos de uma passagem
        /// </summary>
        public async Task<Ticket> GetTicketCompleteAsync(int ticketId)
        {
            return await _context.Tickets
                .AsNoTracking()
                .Include(t => t.Flight)
                    .ThenInclude(f => f.DepartureAirport)
                .Include(t => t.Flight)
                    .ThenInclude(f => f.ArrivalAirport)
                .Include(t => t.Flight)
                    .ThenInclude(f => f.Aircraft)
                .Include(t => t.Customer)
                .Include(t => t.Seat)
                .FirstOrDefaultAsync(t => t.TicketId == ticketId);
        }

        /// <summary>
        /// Verifica se uma poltrona está disponível
        /// </summary>
        public async Task<bool> IsSeatAvailableAsync(int flightId, int seatId)
        {
            var seat = await _context.Seats
                .FirstOrDefaultAsync(s => s.SeatId == seatId && s.FlightId == flightId);

            if (seat == null) return false;

            var isOccupied = await _context.Tickets
                .AnyAsync(t => t.SeatId == seatId && t.Status != TicketStatus.Cancelled);

            return seat.IsAvailable && !isOccupied;
        }

        // =============================================
        // MÉTODOS PRIVADOS AUXILIARES
        // =============================================

        private async Task<Customer> ValidateCustomerAsync(int customerId)
        {
            return await _context.Customers
                .FirstOrDefaultAsync(c => c.CustomerId == customerId && c.IsActive);
        }

        private async Task<Flight> ValidateFlightAsync(int flightId)
        {
            return await _context.Flights
                .FirstOrDefaultAsync(f => f.FlightId == flightId);
        }

        private async Task<Seat> ValidateSeatAsync(int seatId)
        {
            return await _context.Seats
                .FirstOrDefaultAsync(s => s.SeatId == seatId && s.IsAvailable);
        }

        private async Task<bool> IsSeatOccupiedAsync(int seatId)
        {
            return await _context.Tickets
                .AnyAsync(t => t.SeatId == seatId && t.Status != TicketStatus.Cancelled);
        }

        private Ticket CreateTicket(IssueTicketRequestDto request, Seat seat)
        {
            return new Ticket
            {
                CustomerId = request.CustomerId,
                FlightId = request.FlightId,
                SeatId = request.SeatId,
                TicketNumber = GenerateTicketNumber(),
                IssueDate = DateTime.Now,
                Price = seat.Price,
                Status = TicketStatus.Confirmed,
                Class = seat.Class
            };
        }

        private string GenerateTicketNumber()
        {
            return Guid.NewGuid().ToString("N").Substring(0, 20).ToUpper();
        }
    }
}