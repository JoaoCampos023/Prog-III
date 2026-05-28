using System.ComponentModel.DataAnnotations;

namespace SistemaAereo.Models.Entities
{
    public class Stopover
    {
        [Key]
        public int StopoverId { get; set; }

        [Required]
        public int FlightId { get; set; }

        [Required]
        public int AirportId { get; set; }

        [Required]
        [Display(Name = "Ordem")]
        public int Order { get; set; }

        [Required]
        [Display(Name = "Horário de Saída")]
        public DateTime DepartureTime { get; set; }

        [Display(Name = "Horário de Chegada")]
        public DateTime? ArrivalTime { get; set; }

        public virtual Flight Flight { get; set; }
        public virtual Airport Airport { get; set; }
    }
}