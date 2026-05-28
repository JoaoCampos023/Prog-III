using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Net.Sockets;

namespace SistemaAereo.Models.Entities
{
    public class Flight
    {
        [Key]
        public int FlightId { get; set; }

        [Required(ErrorMessage = "O número do voo é obrigatório")]
        [StringLength(10, ErrorMessage = "O número do voo deve ter no máximo 10 caracteres")]
        [Display(Name = "Número do Voo")]
        public string FlightNumber { get; set; }

        [Required(ErrorMessage = "O aeroporto de origem é obrigatório")]
        [Display(Name = "Aeroporto de Origem")]
        public int DepartureAirportId { get; set; }

        [Required(ErrorMessage = "O aeroporto de destino é obrigatório")]
        [Display(Name = "Aeroporto de Destino")]
        public int ArrivalAirportId { get; set; }

        [Required(ErrorMessage = "A aeronave é obrigatória")]
        [Display(Name = "Aeronave")]
        public int AircraftId { get; set; }

        [Required(ErrorMessage = "O horário de saída é obrigatório")]
        [Display(Name = "Horário de Saída")]
        public DateTime DepartureTime { get; set; }

        [Required(ErrorMessage = "O horário de chegada é obrigatório")]
        [Display(Name = "Horário de Chegada Previsto")]
        public DateTime EstimatedArrivalTime { get; set; }

        [ForeignKey("DepartureAirportId")]
        public virtual Airport DepartureAirport { get; set; }

        [ForeignKey("ArrivalAirportId")]
        public virtual Airport ArrivalAirport { get; set; }

        [ForeignKey("AircraftId")]
        public virtual Aircraft Aircraft { get; set; }

        public virtual ICollection<Stopover> Stopovers { get; set; }
        public virtual ICollection<Seat> Seats { get; set; }
        public virtual ICollection<Ticket> Tickets { get; set; }

        public Flight()
        {
            Stopovers = new HashSet<Stopover>();
            Seats = new HashSet<Seat>();
            Tickets = new HashSet<Ticket>();
        }
    }
}