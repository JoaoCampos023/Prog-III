using System.ComponentModel.DataAnnotations;

namespace SistemaAereo.Models.ViewModels
{
    public class ProfileViewModel
    {
        public string Email { get; set; }

        [Required(ErrorMessage = "O nome é obrigatório")]
        [StringLength(100, ErrorMessage = "O nome deve ter no máximo 100 caracteres")]
        [Display(Name = "Nome Completo")]
        public string NomeCompleto { get; set; }

        [Phone(ErrorMessage = "Telefone inválido")]
        [Display(Name = "Telefone")]
        public string Telefone { get; set; }

        [Display(Name = "Data de Cadastro")]
        public DateTime DataCadastro { get; set; }
    }
}