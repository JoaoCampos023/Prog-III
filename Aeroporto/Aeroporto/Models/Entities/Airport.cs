using System.ComponentModel.DataAnnotations;

namespace SistemaAereo.Models.Entities
{
    // Representa um aeroporto cadastrado no sistema
    public class Airport
    {
        // Identificador único do aeroporto
        [Key]
        public int AirportId { get; set; }

        // Nome completo do aeroporto
        [Required]
        [StringLength(100)]
        public string Name { get; set; }

        // Código IATA de 3 letras (ex: GRU, SDU, BSB)
        [Required]
        [StringLength(3)]
        [Display(Name = "Código IATA")]
        public string IATACode { get; set; }

        // Cidade onde o aeroporto está localizado
        [StringLength(100)]
        public string City { get; set; }

        // País onde o aeroporto está localizado
        [StringLength(100)]
        public string Country { get; set; }

        // Voos que partem deste aeroporto (origem)
        public virtual ICollection<Flight> DepartureFlights { get; set; }

        // Voos que chegam neste aeroporto (destino)
        public virtual ICollection<Flight> ArrivalFlights { get; set; }

        // Escalas que ocorrem neste aeroporto
        public virtual ICollection<Stopover> Stopovers { get; set; }

        // Construtor - inicializa as coleções
        public Airport()
        {
            DepartureFlights = new HashSet<Flight>();
            ArrivalFlights = new HashSet<Flight>();
            Stopovers = new HashSet<Stopover>();
        }
    }
}