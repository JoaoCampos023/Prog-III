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
    // Implementação da fachada de passagens
    // Centraliza toda a lógica de negócio relacionada a tickets
    public class TicketFacade : ITicketFacade
    {
        // Dependências injetadas
        private readonly ITicketRepository _ticketRepository;
        private readonly ISeatRepository _seatRepository;
        private readonly ICustomerRepository _customerRepository;
        private readonly IFlightRepository _flightRepository;
        private readonly ISeatService _seatService;
        private readonly AirportsContext _context;
        private readonly ILogger<TicketFacade> _logger;

        // Construtor - recebe todas as dependências via injeção
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

        // Emite uma nova passagem
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

            // Verifica se o voo ainda não partiu
            if (flight.DepartureTime < DateTime.Now)
                return TicketResultDto.Fail("Não é possível emitir passagem para um voo que já partiu");

            // Inicia transação para garantir consistência
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                // Verifica e bloqueia a poltrona
                var seat = await ValidateSeatAsync(request.SeatId);
                if (seat == null)
                    return TicketResultDto.Fail("Poltrona não encontrada ou indisponível");

                // Verifica se a poltrona não está ocupada por outra passagem
                if (await IsSeatOccupiedAsync(seat.SeatId))
                    return TicketResultDto.Fail("Poltrona já foi ocupada por outra passagem");

                // Marca poltrona como indisponível
                seat.IsAvailable = false;
                _context.Seats.Update(seat);
                await _context.SaveChangesAsync();

                // Cria a passagem
                var ticket = CreateTicket(request, seat);
                _context.Tickets.Add(ticket);
                await _context.SaveChangesAsync();

                // Confirma a transação
                await transaction.CommitAsync();

                _logger.LogInformation($"Passagem emitida com sucesso - Bilhete: {ticket.TicketNumber}");
                return TicketResultDto.Ok(ticket);
            }
            catch (DbUpdateConcurrencyException ex)
            {
                // Conflito de concorrência - outro usuário comprou a mesma poltrona
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Conflito de concorrência ao emitir passagem");
                return TicketResultDto.Fail("A poltrona foi comprada por outro usuário. Tente novamente.");
            }
            catch (Exception ex)
            {
                // Erro geral - faz rollback da transação
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Erro ao emitir passagem");
                return TicketResultDto.Fail($"Erro ao processar a compra: {ex.Message}");
            }
        }

        // Cancela uma passagem existente
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

                // Verifica se a passagem já está cancelada
                if (ticket.Status == TicketStatus.Cancelled)
                    return TicketResultDto.Fail("Passagem já está cancelada");

                // Não permite cancelar passagem já embarcada
                if (ticket.Status == TicketStatus.Boarded)
                    return TicketResultDto.Fail("Não é possível cancelar uma passagem já embarcada");

                // Não permite cancelar passagem de voo que já partiu
                if (ticket.Flight != null && ticket.Flight.DepartureTime < DateTime.Now)
                    return TicketResultDto.Fail("Não é possível cancelar uma passagem de um voo que já partiu");

                // Atualiza status da passagem
                ticket.Status = TicketStatus.Cancelled;
                _context.Tickets.Update(ticket);
                await _context.SaveChangesAsync();

                // Libera a poltrona novamente
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

        // Realiza check-in de uma passagem
        public async Task<TicketResultDto> CheckinAsync(CheckinRequestDto request)
        {
            _logger.LogInformation($"Iniciando check-in - Passagem ID: {request.TicketId}");

            try
            {
                var ticket = await _ticketRepository.GetTicketCompleteAsync(request.TicketId);
                if (ticket == null)
                    return TicketResultDto.Fail("Passagem não encontrada");

                // Verifica se o status permite check-in
                if (ticket.Status != TicketStatus.Confirmed)
                    return TicketResultDto.Fail($"Check-in não permitido. Status atual: {ticket.Status}");

                // Impede check-in de voo já partido
                if (ticket.Flight != null && ticket.Flight.DepartureTime < DateTime.Now)
                    return TicketResultDto.Fail("Não é possível fazer check-in de um voo que já partiu");

                // Atualiza status
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

        // Registra embarque de uma passagem
        public async Task<TicketResultDto> RegisterBoardingAsync(int ticketId)
        {
            _logger.LogInformation($"Registrando embarque - Passagem ID: {ticketId}");

            try
            {
                var ticket = await _ticketRepository.GetByIdAsync(ticketId);
                if (ticket == null)
                    return TicketResultDto.Fail("Passagem não encontrada");

                // Verifica se o check-in já foi realizado
                if (ticket.Status != TicketStatus.CheckIn)
                    return TicketResultDto.Fail($"Embarque não permitido. Status atual: {ticket.Status}. É necessário fazer check-in primeiro.");

                // Atualiza status
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

        // Obtém detalhes completos de uma passagem
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

        // Verifica se uma poltrona está disponível
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

        // Valida se o cliente existe e está ativo
        private async Task<Customer> ValidateCustomerAsync(int customerId)
        {
            return await _context.Customers
                .FirstOrDefaultAsync(c => c.CustomerId == customerId && c.IsActive);
        }

        // Valida se o voo existe
        private async Task<Flight> ValidateFlightAsync(int flightId)
        {
            return await _context.Flights
                .FirstOrDefaultAsync(f => f.FlightId == flightId);
        }

        // Valida se a poltrona existe e está disponível
        private async Task<Seat> ValidateSeatAsync(int seatId)
        {
            return await _context.Seats
                .FirstOrDefaultAsync(s => s.SeatId == seatId && s.IsAvailable);
        }

        // Verifica se a poltrona já está ocupada por outra passagem
        private async Task<bool> IsSeatOccupiedAsync(int seatId)
        {
            return await _context.Tickets
                .AnyAsync(t => t.SeatId == seatId && t.Status != TicketStatus.Cancelled);
        }

        // Cria uma nova passagem com os dados fornecidos
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

        // Gera um número de bilhete único usando GUID
        private string GenerateTicketNumber()
        {
            return Guid.NewGuid().ToString("N").Substring(0, 20).ToUpper();
        }
    }
}