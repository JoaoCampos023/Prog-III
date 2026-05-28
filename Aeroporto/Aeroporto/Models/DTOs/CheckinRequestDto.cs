namespace SistemaAereo.Models.DTOs
{
    public class CheckinRequestDto
    {
        public int TicketId { get; set; }
        public int NumberOfBags { get; set; }
        public decimal BaggageWeight { get; set; }
    }
}