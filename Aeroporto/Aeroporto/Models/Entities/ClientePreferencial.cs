using System.ComponentModel.DataAnnotations;

namespace SistemaAereo.Models.Entities
{
    public class ClientePreferencial
    {
        // =============================================
        // PROPRIEDADES PRINCIPAIS
        // =============================================

        [Key]
        public int ClienteId { get; set; }

        [Required]
        [StringLength(100)]
        public string Nome { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [StringLength(20)]
        public string Telefone { get; set; }

        [StringLength(14)]
        public string CPF { get; set; }

        [DataType(DataType.Date)]
        public DateTime? DataNascimento { get; set; }

        // =============================================
        // PROPRIEDADES DE ENDEREÇO
        // =============================================

        [StringLength(200)]
        public string Endereco { get; set; }

        [StringLength(100)]
        public string Cidade { get; set; }

        [StringLength(2)]
        public string Estado { get; set; }

        [StringLength(9)]
        public string CEP { get; set; }

        // =============================================
        // PROPRIEDADES DE CONTROLE
        // =============================================

        public DateTime DataCadastro { get; set; } = DateTime.Now;
        public bool Ativo { get; set; } = true;
    }
}