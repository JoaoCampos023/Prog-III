using System.ComponentModel.DataAnnotations;

namespace SistemaAereo.Models.Entities
{
    public class Passagem
    {
        // =============================================
        // PROPRIEDADES DE IDENTIFICAÇÃO
        // =============================================

        [Key]
        public int PassagemId { get; set; }

        [Required]
        public int VooId { get; set; }

        [Required]
        public int ClienteId { get; set; }

        [Required]
        public int PoltronaId { get; set; }

        [Required]
        [StringLength(20)]
        public string NumeroBilhete { get; set; }

        // =============================================
        // PROPRIEDADES DE DADOS DA PASSAGEM
        // =============================================

        [Required]
        public DateTime DataEmissao { get; set; } = DateTime.Now;

        [Required]
        [DataType(DataType.Currency)]
        public decimal Preco { get; set; }

        [StringLength(20)]
        public string Status { get; set; } = "Confirmada"; // Confirmada, Check-in, Embarcada, Cancelada

        [StringLength(50)]
        public string Classe { get; set; } // Economica, Executiva, Primeira

        // =============================================
        // RELACIONAMENTOS
        // =============================================

        public virtual Voo Voo { get; set; }
        public virtual ClientePreferencial Cliente { get; set; }
        public virtual Poltrona Poltrona { get; set; }
    }
}