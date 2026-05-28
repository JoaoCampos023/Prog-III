using System.ComponentModel.DataAnnotations;

namespace SistemaAereo.Models.DTOs
{
    public class IssueTicketRequestDto
    {
        [Required(ErrorMessage = "Cliente é obrigatório")]
        public int CustomerId { get; set; }

        [Required(ErrorMessage = "Voo é obrigatório")]
        public int FlightId { get; set; }

        [Required(ErrorMessage = "Poltrona é obrigatória")]
        public int SeatId { get; set; }
    }
}