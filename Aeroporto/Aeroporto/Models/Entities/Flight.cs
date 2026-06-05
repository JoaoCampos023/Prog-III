using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SistemaAereo.Models.Entities
{
    // Representa um voo programado
    public class Flight
    {
        // Identificador único do voo
        [Key]
        public int FlightId { get; set; }

        // Número identificador do voo (ex: LA1234)
        [Required(ErrorMessage = "O número do voo é obrigatório")]
        [StringLength(10, ErrorMessage = "O número do voo deve ter no máximo 10 caracteres")]
        [Display(Name = "Número do Voo")]
        public string FlightNumber { get; set; }

        // ID do aeroporto de origem (chave estrangeira)
        [Required(ErrorMessage = "O aeroporto de origem é obrigatório")]
        [Display(Name = "Aeroporto de Origem")]
        public int DepartureAirportId { get; set; }

        // ID do aeroporto de destino (chave estrangeira)
        [Required(ErrorMessage = "O aeroporto de destino é obrigatório")]
        [Display(Name = "Aeroporto de Destino")]
        public int ArrivalAirportId { get; set; }

        // ID da aeronave utilizada (chave estrangeira)
        [Required(ErrorMessage = "A aeronave é obrigatória")]
        [Display(Name = "Aeronave")]
        public int AircraftId { get; set; }

        // Data e hora de saída do voo
        [Required(ErrorMessage = "O horário de saída é obrigatório")]
        [Display(Name = "Horário de Saída")]
        public DateTime DepartureTime { get; set; }

        // Data e hora prevista de chegada
        [Required(ErrorMessage = "O horário de chegada é obrigatório")]
        [Display(Name = "Horário de Chegada Previsto")]
        public DateTime EstimatedArrivalTime { get; set; }

        // =============================================
        // RELACIONAMENTOS
        // =============================================

        // Aeroporto de origem (navegação)
        [ForeignKey("DepartureAirportId")]
        public virtual Airport DepartureAirport { get; set; }

        // Aeroporto de destino (navegação)
        [ForeignKey("ArrivalAirportId")]
        public virtual Airport ArrivalAirport { get; set; }

        // Aeronave utilizada (navegação)
        [ForeignKey("AircraftId")]
        public virtual Aircraft Aircraft { get; set; }

        // Lista de escalas do voo
        public virtual ICollection<Stopover> Stopovers { get; set; }

        // Lista de poltronas do voo
        public virtual ICollection<Seat> Seats { get; set; }

        // Lista de passagens vendidas para este voo
        public virtual ICollection<Ticket> Tickets { get; set; }

        // Construtor - inicializa as coleções
        public Flight()
        {
            Stopovers = new HashSet<Stopover>();
            Seats = new HashSet<Seat>();
            Tickets = new HashSet<Ticket>();
        }
    }
}