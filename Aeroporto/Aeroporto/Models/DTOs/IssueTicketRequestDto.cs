using System.ComponentModel.DataAnnotations;

namespace SistemaAereo.Models.DTOs
{
    // DTO para requisição de emissão de passagem
    public class IssueTicketRequestDto
    {
        // ID do cliente que está comprando a passagem
        [Required(ErrorMessage = "Cliente é obrigatório")]
        public int CustomerId { get; set; }

        // ID do voo escolhido
        [Required(ErrorMessage = "Voo é obrigatório")]
        public int FlightId { get; set; }

        // ID da poltrona selecionada
        [Required(ErrorMessage = "Poltrona é obrigatória")]
        public int SeatId { get; set; }
    }
}