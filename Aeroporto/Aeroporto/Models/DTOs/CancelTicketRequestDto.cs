namespace SistemaAereo.Models.DTOs
{
    // DTO para requisição de cancelamento de passagem
    public class CancelTicketRequestDto
    {
        // ID da passagem a ser cancelada
        public int TicketId { get; set; }

        // Motivo do cancelamento (opcional)
        public string Reason { get; set; }
    }
}