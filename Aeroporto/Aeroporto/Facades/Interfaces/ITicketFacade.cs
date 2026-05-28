using SistemaAereo.Models.DTOs;
using SistemaAereo.Models.Entities;

namespace SistemaAereo.Facades.Interfaces
{
    /// <summary>
    /// Fachada para operações complexas relacionadas a passagens
    /// </summary>
    public interface ITicketFacade
    {
        /// <summary>
        /// Emite uma nova passagem
        /// </summary>
        Task<TicketResultDto> IssueTicketAsync(IssueTicketRequestDto request);

        /// <summary>
        /// Cancela uma passagem existente
        /// </summary>
        Task<TicketResultDto> CancelTicketAsync(CancelTicketRequestDto request);

        /// <summary>
        /// Realiza check-in de uma passagem
        /// </summary>
        Task<TicketResultDto> CheckinAsync(CheckinRequestDto request);

        /// <summary>
        /// Registra embarque de uma passagem
        /// </summary>
        Task<TicketResultDto> RegisterBoardingAsync(int ticketId);

        /// <summary>
        /// Obtém detalhes completos de uma passagem
        /// </summary>
        Task<Ticket> GetTicketCompleteAsync(int ticketId);

        /// <summary>
        /// Verifica se uma poltrona está disponível
        /// </summary>
        Task<bool> IsSeatAvailableAsync(int flightId, int seatId);
    }
}