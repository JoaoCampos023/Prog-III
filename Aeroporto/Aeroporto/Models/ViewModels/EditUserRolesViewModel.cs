using System.ComponentModel.DataAnnotations;

namespace SistemaAereo.Models.ViewModels
{
    // ViewModel para edição de roles/permissões de um usuário
    public class EditUserRolesViewModel
    {
        // ID do usuário a ser editado
        public string UserId { get; set; }

        // Nome de usuário (login)
        [Display(Name = "Nome de Usuário")]
        public string UserName { get; set; }

        // Email do usuário
        [Display(Name = "Email")]
        public string UserEmail { get; set; }

        // Lista de roles que o usuário já possui
        [Display(Name = "Roles Atuais")]
        public List<string> CurrentRoles { get; set; } = new List<string>();

        // Lista de todas as roles disponíveis no sistema
        [Display(Name = "Roles Disponíveis")]
        public List<string> AllRoles { get; set; } = new List<string>();

        // Lista de roles selecionadas para atribuir ao usuário
        [Display(Name = "Selecionar Roles")]
        public List<string> SelectedRoles { get; set; } = new List<string>();
    }
}