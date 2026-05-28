using System.ComponentModel.DataAnnotations;

namespace SistemaAereo.Models.Entities
{
    public class Aircraft
    {
        // =============================================
        // PROPRIEDADES PRINCIPAIS
        // =============================================

        [Key]
        public int AircraftId { get; set; }

        [Required]
        [StringLength(100)]
        [Display(Name = "Tipo de Aeronave")]
        public string AircraftType { get; set; }

        [Required]
        [Display(Name = "Número de Poltronas")]
        public int NumberOfSeats { get; set; }

        // =============================================
        // RELACIONAMENTOS
        // =============================================

        public virtual ICollection<Flight> Flights { get; set; }

        // =============================================
        // CONSTRUTOR
        // =============================================

        public Aircraft()
        {
            Flights = new HashSet<Flight>();
        }
    }
}