using System.ComponentModel.DataAnnotations;

namespace SistemaAereo.Models.ViewModels
{
    // ViewModel para a página de alteração de senha
    public class ChangePasswordViewModel
    {
        // Senha atual do usuário (requerida para validar a alteração)
        [Required(ErrorMessage = "A senha atual é obrigatória")]
        [DataType(DataType.Password)]
        [Display(Name = "Senha Atual")]
        public string CurrentPassword { get; set; }

        // Nova senha (com validação de força)
        [Required(ErrorMessage = "A nova senha é obrigatória")]
        [StringLength(100, ErrorMessage = "A senha deve ter no mínimo 6 caracteres", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Nova Senha")]
        public string NewPassword { get; set; }

        // Confirmação da nova senha
        [DataType(DataType.Password)]
        [Display(Name = "Confirmar Nova Senha")]
        [Compare("NewPassword", ErrorMessage = "As senhas não conferem")]
        public string ConfirmPassword { get; set; }
    }
}