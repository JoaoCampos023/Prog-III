using System.ComponentModel.DataAnnotations;

namespace SistemaAereo.Models.Entities
{
    public class Airport
    {
        // =============================================
        // PROPRIEDADES PRINCIPAIS
        // =============================================

        [Key]
        public int AirportId { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; }

        [Required]
        [StringLength(3)]
        [Display(Name = "Código IATA")]
        public string IATACode { get; set; }

        [StringLength(100)]
        public string City { get; set; }

        [StringLength(100)]
        public string Country { get; set; }

        // =============================================
        // RELACIONAMENTOS
        // =============================================

        public virtual ICollection<Flight> DepartureFlights { get; set; }
        public virtual ICollection<Flight> ArrivalFlights { get; set; }
        public virtual ICollection<Stopover> Stopovers { get; set; }

        // =============================================
        // CONSTRUTOR
        // =============================================

        public Airport()
        {
            DepartureFlights = new HashSet<Flight>();
            ArrivalFlights = new HashSet<Flight>();
            Stopovers = new HashSet<Stopover>();
        }
    }
}