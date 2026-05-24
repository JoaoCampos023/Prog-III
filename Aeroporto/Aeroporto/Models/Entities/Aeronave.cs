using System.ComponentModel.DataAnnotations;

namespace SistemaAereo.Models.Entities
{
    public class Aeronave
    {
        // =============================================
        // PROPRIEDADES PRINCIPAIS
        // =============================================

        [Key]
        public int AeronaveId { get; set; }

        [Required]
        [StringLength(100)]
        public string TipoAeronave { get; set; }

        [Required]
        public int NumeroPoltronas { get; set; }

        // =============================================
        // RELACIONAMENTOS
        // =============================================

        public virtual ICollection<Voo> Voos { get; set; }

        // =============================================
        // CONSTRUTOR
        // =============================================

        public Aeronave()
        {
            Voos = new HashSet<Voo>();
        }
    }
}