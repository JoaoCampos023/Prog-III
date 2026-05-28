using System.ComponentModel.DataAnnotations;
using System.Net.Sockets;

namespace SistemaAereo.Models.Entities
{
    public class Seat
    {
        [Key]
        public int SeatId { get; set; }

        [Required]
        public int FlightId { get; set; }

        [Required]
        [StringLength(10)]
        [Display(Name = "Número da Poltrona")]
        public string SeatNumber { get; set; }

        [Required]
        [Display(Name = "Disponível")]
        public bool IsAvailable { get; set; } = true;

        [Required]
        [StringLength(20)]
        [Display(Name = "Localização")]
        public string Location { get; set; }

        [StringLength(20)]
        [Display(Name = "Tipo")]
        public string Class { get; set; }

        [DataType(DataType.Currency)]
        [Display(Name = "Preço")]
        public decimal Price { get; set; }

        [Timestamp]
        public byte[] RowVersion { get; set; }

        public virtual ICollection<Ticket> Tickets { get; set; }
        public virtual Flight Flight { get; set; }

        public Seat()
        {
            Tickets = new HashSet<Ticket>();
        }
    }
}