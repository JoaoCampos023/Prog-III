using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace SistemaAereo.Models.Entities
{
    // Representa um usuário do sistema (herda do IdentityUser)
    public class User : IdentityUser
    {
        // Nome completo do usuário
        [Display(Name = "Nome Completo")]
        public string FullName { get; set; }

        // Data em que o usuário foi cadastrado
        [Display(Name = "Data de Cadastro")]
        public DateTime RegistrationDate { get; set; } = DateTime.Now;

        // Indica se o usuário está ativo no sistema
        [Display(Name = "Ativo")]
        public bool IsActive { get; set; } = true;

        // URL do avatar do usuário (pode ser nulo)
        [Display(Name = "Avatar")]
        public string? AvatarUrl { get; set; }
    }
}