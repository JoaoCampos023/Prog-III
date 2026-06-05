namespace SistemaAereo.Models.DTOs
{
    // DTO para requisição de check-in
    public class CheckinRequestDto
    {
        // ID da passagem para realizar check-in
        public int TicketId { get; set; }

        // Número de bagagens (opcional)
        public int NumberOfBags { get; set; }

        // Peso total das bagagens (opcional)
        public decimal BaggageWeight { get; set; }
    }
}