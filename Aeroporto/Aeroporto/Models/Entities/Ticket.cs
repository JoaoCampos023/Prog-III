using System.ComponentModel.DataAnnotations;
using SistemaAereo.Models.Enums; 

namespace SistemaAereo.Models.Entities
{
    public class Ticket
    {
        [Key]
        public int TicketId { get; set; }

        [Required]
        public int FlightId { get; set; }

        [Required]
        public int CustomerId { get; set; }

        [Required]
        public int SeatId { get; set; }

        [Required]
        [StringLength(20)]
        [Display(Name = "Número do Bilhete")]
        public string TicketNumber { get; set; }

        [Required]
        [Display(Name = "Data de Emissão")]
        public DateTime IssueDate { get; set; } = DateTime.Now;

        [Required]
        [DataType(DataType.Currency)]
        [Display(Name = "Preço")]
        public decimal Price { get; set; }

        [StringLength(20)]
        [Display(Name = "Status")]
        public string Status { get; set; } = TicketStatus.Confirmed;

        [StringLength(50)]
        [Display(Name = "Classe")]
        public string Class { get; set; }

        public virtual Flight Flight { get; set; }
        public virtual Customer Customer { get; set; }
        public virtual Seat Seat { get; set; }
    }
}