using SistemaAereo.Models.Entities;

namespace SistemaAereo.Models.DTOs
{
    // DTO para resultado da operação de emissão/cancelamento de passagem
    public class TicketResultDto
    {
        // Indica se a operação foi bem sucedida
        public bool Success { get; set; }

        // Mensagem amigável sobre o resultado
        public string Message { get; set; }

        // Mensagem de erro (quando Success = false)
        public string ErrorMessage { get; set; }

        // Objeto Ticket completo (quando Success = true)
        public Ticket Ticket { get; set; }

        // ID da passagem criada
        public int TicketId { get; set; }

        // Número do bilhete gerado
        public string TicketNumber { get; set; }

        // Cria um resultado de sucesso com a passagem
        public static TicketResultDto Ok(Ticket ticket)
        {
            return new TicketResultDto
            {
                Success = true,
                Message = "Operação realizada com sucesso",
                Ticket = ticket,
                TicketId = ticket.TicketId,
                TicketNumber = ticket.TicketNumber
            };
        }

        // Cria um resultado de falha com mensagem de erro
        public static TicketResultDto Fail(string errorMessage)
        {
            return new TicketResultDto
            {
                Success = false,
                ErrorMessage = errorMessage,
                Message = "Falha na operação"
            };
        }

        // Resultado de sucesso para cancelamento
        public static TicketResultDto CancelOk()
        {
            return new TicketResultDto
            {
                Success = true,
                Message = "Passagem cancelada com sucesso"
            };
        }

        // Resultado de sucesso para check-in
        public static TicketResultDto CheckinOk()
        {
            return new TicketResultDto
            {
                Success = true,
                Message = "Check-in realizado com sucesso"
            };
        }

        // Resultado de sucesso para embarque
        public static TicketResultDto BoardingOk()
        {
            return new TicketResultDto
            {
                Success = true,
                Message = "Embarque registrado com sucesso"
            };
        }
    }
}