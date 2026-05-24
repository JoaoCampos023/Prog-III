using System.ComponentModel.DataAnnotations;

namespace SistemaAereo.Models.Entities
{
    public class Escala
    {
        // =============================================
        // PROPRIEDADES PRINCIPAIS
        // =============================================

        [Key]
        public int EscalaId { get; set; }

        [Required]
        public int VooId { get; set; }

        [Required]
        public int AeroportoId { get; set; }

        [Required]
        public int Ordem { get; set; }

        // =============================================
        // PROPRIEDADES DE HORÁRIO
        // =============================================

        [Required]
        public DateTime HorarioSaida { get; set; }

        public DateTime? HorarioChegada { get; set; }

        // =============================================
        // RELACIONAMENTOS
        // =============================================

        public virtual Voo Voo { get; set; }
        public virtual Aeroporto Aeroporto { get; set; }
    }
}