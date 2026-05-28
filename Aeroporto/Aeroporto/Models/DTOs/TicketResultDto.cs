using SistemaAereo.Models.Entities;

namespace SistemaAereo.Models.DTOs
{
    public class TicketResultDto
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public string ErrorMessage { get; set; }
        public Ticket Ticket { get; set; }
        public int TicketId { get; set; }
        public string TicketNumber { get; set; }

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

        public static TicketResultDto Fail(string errorMessage)
        {
            return new TicketResultDto
            {
                Success = false,
                ErrorMessage = errorMessage,
                Message = "Falha na operação"
            };
        }

        public static TicketResultDto CancelOk()
        {
            return new TicketResultDto
            {
                Success = true,
                Message = "Passagem cancelada com sucesso"
            };
        }

        public static TicketResultDto CheckinOk()
        {
            return new TicketResultDto
            {
                Success = true,
                Message = "Check-in realizado com sucesso"
            };
        }

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