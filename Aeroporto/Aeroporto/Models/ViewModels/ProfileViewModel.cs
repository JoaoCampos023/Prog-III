using System.ComponentModel.DataAnnotations;

namespace SistemaAereo.Models.ViewModels
{
    public class ProfileViewModel
    {
        public string Email { get; set; }

        [Required(ErrorMessage = "O nome completo é obrigatório")]
        [StringLength(100)]
        [Display(Name = "Nome Completo")]
        public string FullName { get; set; }  // antes NomeCompleto

        [Phone(ErrorMessage = "Telefone inválido")]
        [Display(Name = "Telefone")]
        public string Phone { get; set; }  // antes Telefone

        [Display(Name = "Data de Cadastro")]
        public DateTime RegistrationDate { get; set; }  // antes DataCadastro
    }
}