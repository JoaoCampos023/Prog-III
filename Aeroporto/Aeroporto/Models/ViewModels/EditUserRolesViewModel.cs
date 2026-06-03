using System.ComponentModel.DataAnnotations;

namespace SistemaAereo.Models.ViewModels
{
    public class EditUserRolesViewModel
    {
        public string UserId { get; set; }

        [Display(Name = "Nome de Usuário")]
        public string UserName { get; set; }

        [Display(Name = "Email")]
        public string UserEmail { get; set; }

        [Display(Name = "Roles Atuais")]
        public List<string> CurrentRoles { get; set; } = new List<string>();

        [Display(Name = "Roles Disponíveis")]
        public List<string> AllRoles { get; set; } = new List<string>();

        [Display(Name = "Selecionar Roles")]
        public List<string> SelectedRoles { get; set; } = new List<string>();
    }
}