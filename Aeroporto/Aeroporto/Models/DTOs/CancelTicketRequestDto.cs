namespace SistemaAereo.Models.DTOs
{
    public class CancelTicketRequestDto
    {
        public int TicketId { get; set; }
        public string Reason { get; set; }
    }
}