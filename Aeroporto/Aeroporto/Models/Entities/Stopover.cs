using System.ComponentModel.DataAnnotations;

namespace SistemaAereo.Models.Entities
{
    // Representa uma escala/interligação entre voos
    public class Stopover
    {
        // Identificador único da escala
        [Key]
        public int StopoverId { get; set; }

        // ID do voo principal (chave estrangeira)
        [Required]
        public int FlightId { get; set; }

        // ID do aeroporto onde ocorre a escala
        [Required]
        public int AirportId { get; set; }

        // Ordem da escala na sequência do voo
        [Required]
        [Display(Name = "Ordem")]
        public int Order { get; set; }

        // Horário de saída da escala
        [Required]
        [Display(Name = "Horário de Saída")]
        public DateTime DepartureTime { get; set; }

        // Horário de chegada na escala (opcional)
        [Display(Name = "Horário de Chegada")]
        public DateTime? ArrivalTime { get; set; }

        // =============================================
        // RELACIONAMENTOS
        // =============================================

        // Voo principal (navegação)
        public virtual Flight Flight { get; set; }

        // Aeroporto da escala (navegação)
        public virtual Airport Airport { get; set; }
    }
}