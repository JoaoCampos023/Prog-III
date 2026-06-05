using System.ComponentModel.DataAnnotations;

namespace SistemaAereo.Models.ViewModels
{
    // ViewModel para a página de perfil do usuário
    public class ProfileViewModel
    {
        // Email do usuário (somente leitura)
        public string Email { get; set; }

        // Nome completo do usuário
        [Required(ErrorMessage = "O nome completo é obrigatório")]
        [StringLength(100)]
        [Display(Name = "Nome Completo")]
        public string FullName { get; set; }

        // Telefone para contato
        [Phone(ErrorMessage = "Telefone inválido")]
        [Display(Name = "Telefone")]
        public string Phone { get; set; }

        // Data de cadastro do usuário (somente leitura)
        [Display(Name = "Data de Cadastro")]
        public DateTime RegistrationDate { get; set; }
    }
}