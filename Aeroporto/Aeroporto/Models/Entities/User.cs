using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace SistemaAereo.Models.Entities
{
    public class User : IdentityUser
    {
        [Display(Name = "Nome Completo")]
        public string FullName { get; set; }

        [Display(Name = "Data de Cadastro")]
        public DateTime RegistrationDate { get; set; } = DateTime.Now;

        [Display(Name = "Ativo")]
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// URL do avatar do usuário (pode ser nulo)
        /// </summary>
        [Display(Name = "Avatar")]
        public string? AvatarUrl { get; set; }  // ← TORNAR NULLABLE com '?'
    }
}