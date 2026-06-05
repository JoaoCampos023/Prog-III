using System.ComponentModel.DataAnnotations;

namespace SistemaAereo.Models.ViewModels
{
    // ViewModel para a página de login
    public class LoginViewModel
    {
        // Email do usuário
        [Required(ErrorMessage = "O email é obrigatório")]
        [EmailAddress(ErrorMessage = "Email inválido")]
        [Display(Name = "Email")]
        public string Email { get; set; }

        // Senha do usuário
        [Required(ErrorMessage = "A senha é obrigatória")]
        [DataType(DataType.Password)]
        [Display(Name = "Senha")]
        public string Password { get; set; }

        // Indica se o usuário quer ser lembrado pelo sistema
        [Display(Name = "Lembrar-me")]
        public bool RememberMe { get; set; }
    }
}