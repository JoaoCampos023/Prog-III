using System.ComponentModel.DataAnnotations;

namespace SistemaAereo.Models.Entities
{
    // Representa uma aeronave da frota
    public class Aircraft
    {
        // Identificador único da aeronave
        [Key]
        public int AircraftId { get; set; }

        // Tipo/modelo da aeronave (ex: Boeing 737-800)
        [Required]
        [StringLength(100)]
        [Display(Name = "Tipo de Aeronave")]
        public string AircraftType { get; set; }

        // Número total de poltronas/assentos da aeronave
        [Required]
        [Display(Name = "Número de Poltronas")]
        public int NumberOfSeats { get; set; }

        // Lista de voos que utilizam esta aeronave
        public virtual ICollection<Flight> Flights { get; set; }

        // Construtor - inicializa a coleção de voos
        public Aircraft()
        {
            Flights = new HashSet<Flight>();
        }
    }
}