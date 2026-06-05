using SistemaAereo.Models.DTOs;
using SistemaAereo.Models.Entities;

namespace SistemaAereo.Facades.Interfaces
{
    // Fachada para operações complexas relacionadas a passagens
    // Simplifica as operações de emissão, cancelamento, check-in e embarque
    public interface ITicketFacade
    {
        // Emite uma nova passagem
        // Recebe os dados da requisição e retorna o resultado da operação
        Task<TicketResultDto> IssueTicketAsync(IssueTicketRequestDto request);

        // Cancela uma passagem existente
        // Libera a poltrona e atualiza o status da passagem
        Task<TicketResultDto> CancelTicketAsync(CancelTicketRequestDto request);

        // Realiza check-in de uma passagem
        // Altera o status da passagem para "Check-in"
        Task<TicketResultDto> CheckinAsync(CheckinRequestDto request);

        // Registra embarque de uma passagem
        // Altera o status da passagem para "Embarcada"
        Task<TicketResultDto> RegisterBoardingAsync(int ticketId);

        // Obtém detalhes completos de uma passagem
        // Inclui dados do voo, cliente, poltrona, etc.
        Task<Ticket> GetTicketCompleteAsync(int ticketId);

        // Verifica se uma poltrona está disponível para venda
        Task<bool> IsSeatAvailableAsync(int flightId, int seatId);
    }
}