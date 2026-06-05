using System.ComponentModel.DataAnnotations;
using SistemaAereo.Models.Enums;

namespace SistemaAereo.Models.Entities
{
    // Representa uma passagem/bilhete emitido
    public class Ticket
    {
        // Identificador único da passagem
        [Key]
        public int TicketId { get; set; }

        // ID do voo (chave estrangeira)
        [Required]
        public int FlightId { get; set; }

        // ID do cliente (chave estrangeira)
        [Required]
        public int CustomerId { get; set; }

        // ID da poltrona (chave estrangeira)
        [Required]
        public int SeatId { get; set; }

        // Número único do bilhete
        [Required]
        [StringLength(20)]
        [Display(Name = "Número do Bilhete")]
        public string TicketNumber { get; set; }

        // Data e hora de emissão da passagem
        [Required]
        [Display(Name = "Data de Emissão")]
        public DateTime IssueDate { get; set; } = DateTime.Now;

        // Preço pago pela passagem
        [Required]
        [DataType(DataType.Currency)]
        [Display(Name = "Preço")]
        public decimal Price { get; set; }

        // Status atual da passagem (Confirmada, Check-in, Embarcada, Cancelada)
        [StringLength(20)]
        [Display(Name = "Status")]
        public string Status { get; set; } = TicketStatus.Confirmed;

        // Classe da passagem (Economica, Executiva, Primeira)
        [StringLength(50)]
        [Display(Name = "Classe")]
        public string Class { get; set; }

        // =============================================
        // RELACIONAMENTOS
        // =============================================

        // Voo associado (navegação)
        public virtual Flight Flight { get; set; }

        // Cliente associado (navegação)
        public virtual Customer Customer { get; set; }

        // Poltrona associada (navegação)
        public virtual Seat Seat { get; set; }
    }
}