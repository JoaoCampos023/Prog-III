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

        [Required(ErrorMessage = "O nome é obrigatório")]
        [StringLength(100, ErrorMessage = "O nome deve ter no máximo 100 caracteres")]
        [Display(Name = "Nome Completo")]
        public string Nome { get; set; }

        [Required(ErrorMessage = "O email é obrigatório")]
        [EmailAddress(ErrorMessage = "Email inválido")]
        [StringLength(100)]
        [Display(Name = "Email")]
        public string Email { get; set; }

        [StringLength(20)]
        [Display(Name = "Telefone")]
        [Phone(ErrorMessage = "Telefone inválido")]
        public string Telefone { get; set; }

        [StringLength(14)]
        [Display(Name = "CPF")]
        public string CPF { get; set; }

        [DataType(DataType.Date)]
        [Display(Name = "Data de Nascimento")]
        public DateTime? DataNascimento { get; set; }

        // =============================================
        // PROPRIEDADES DE ENDEREÇO
        // =============================================

        [StringLength(200)]
        [Display(Name = "Endereço")]
        public string Endereco { get; set; }

        [StringLength(100)]
        [Display(Name = "Cidade")]
        public string Cidade { get; set; }

        [StringLength(2)]
        [Display(Name = "Estado")]
        public string Estado { get; set; }

        [StringLength(9)]
        [Display(Name = "CEP")]
        public string CEP { get; set; }

        // =============================================
        // PROPRIEDADES DE CONTROLE
        // =============================================

        [Display(Name = "Data de Cadastro")]
        public DateTime DataCadastro { get; set; } = DateTime.Now;

        [Display(Name = "Ativo")]
        public bool Ativo { get; set; } = true;

        // =============================================
        // PROPRIEDADE CALCULADA
        // =============================================

        [Display(Name = "Endereço Completo")]
        public string EnderecoCompleto
        {
            get
            {
                if (string.IsNullOrEmpty(Endereco) && string.IsNullOrEmpty(Cidade))
                    return "Endereço não informado";

                var partes = new List<string>();
                if (!string.IsNullOrEmpty(Endereco)) partes.Add(Endereco);
                if (!string.IsNullOrEmpty(Cidade)) partes.Add(Cidade);
                if (!string.IsNullOrEmpty(Estado)) partes.Add(Estado);
                if (!string.IsNullOrEmpty(CEP)) partes.Add($"CEP: {CEP}");

                return string.Join(", ", partes);
            }
        }
    }
}