using System.ComponentModel.DataAnnotations;

namespace SistemaAereo.Models
{
    public class Poltrona
    {
        // =============================================
        // PROPRIEDADES PRINCIPAIS
        // =============================================

        [Key]
        public int PoltronaId { get; set; }

        [Required]
        public int VooId { get; set; }

        [Required]
        [StringLength(10)]
        public string NumeroPoltrona { get; set; }

        [Required]
        public bool Disponivel { get; set; } = true;

        // =============================================
        // PROPRIEDADES DE CARACTERÍSTICAS
        // =============================================

        [Required]
        [StringLength(20)]
        public string Localizacao { get; set; } // "Janela", "Corredor", "Meio"

        [StringLength(20)]
        public string Tipo { get; set; } // "Economica", "Executiva", "Primeira"

        [DataType(DataType.Currency)]
        public decimal Preco { get; set; }

        // =============================================
        // RELACIONAMENTOS
        // =============================================

        public virtual ICollection<Passagem> Passagens { get; set; }
        public virtual Voo Voo { get; set; }

        // =============================================
        // CONSTRUTOR
        // =============================================

        public Poltrona()
        {
            Passagens = new HashSet<Passagem>();
        }
    }
}