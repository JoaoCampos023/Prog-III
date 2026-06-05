using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaAereo.Data.Context;
using SistemaAereo.Models.Entities;
using SistemaAereo.Models.ViewModels;

namespace SistemaAereo.Controllers
{
    [Authorize(Roles = "Admin")]
    public class UserManagementController : Controller
    {
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly AirportsContext _context;
        private readonly ILogger<UserManagementController> _logger;

        public UserManagementController(
            UserManager<User> userManager,
            RoleManager<IdentityRole> roleManager,
            AirportsContext context,
            ILogger<UserManagementController> logger)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _context = context;
            _logger = logger;
        }

        // Lista todos os usuários do sistema com filtro por status
        public async Task<IActionResult> Index(string filter = "todos")
        {
            var users = new List<User>();

            // Aplica filtro por status (todos, ativos, inativos)
            switch (filter?.ToLower())
            {
                case "ativos":
                    users = await _userManager.Users.Where(u => u.IsActive).ToListAsync();
                    break;
                case "inativos":
                    users = await _userManager.Users.Where(u => !u.IsActive).ToListAsync();
                    break;
                default:
                    users = await _userManager.Users.ToListAsync();
                    break;
            }

            // Busca as roles de cada usuário
            var userRoles = new Dictionary<string, IList<string>>();

            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                userRoles[user.Id] = roles;
            }

            ViewBag.UserRoles = userRoles;
            ViewBag.AllRoles = await _roleManager.Roles.ToListAsync();
            ViewBag.CurrentFilter = filter;

            // Estatísticas para os cards
            ViewBag.TotalUsers = await _userManager.Users.CountAsync();
            ViewBag.ActiveUsers = await _userManager.Users.CountAsync(u => u.IsActive);
            ViewBag.InactiveUsers = await _userManager.Users.CountAsync(u => !u.IsActive);

            return View(users);
        }

        // Atalho para listar apenas usuários ativos
        public async Task<IActionResult> Ativos()
        {
            return RedirectToAction("Index", new { filter = "ativos" });
        }

        // Atalho para listar apenas usuários inativos
        public async Task<IActionResult> Inativos()
        {
            return RedirectToAction("Index", new { filter = "inativos" });
        }

        // Formulário para editar as roles de um usuário
        [HttpGet]
        public async Task<IActionResult> EditRoles(string id)
        {
            if (id == null)
                return NotFound();

            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
                return NotFound();

            var userRoles = await _userManager.GetRolesAsync(user);

            // Lista fixa de roles disponíveis
            var allRoles = new List<string> { "Admin", "Funcionario", "User" };

            var model = new EditUserRolesViewModel
            {
                UserId = user.Id,
                UserName = user.UserName,
                UserEmail = user.Email,
                CurrentRoles = userRoles.ToList(),
                AllRoles = allRoles
            };

            return View(model);
        }

        // Salva as alterações de roles do usuário
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditRoles(EditUserRolesViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = await _userManager.FindByIdAsync(model.UserId);
            if (user == null)
                return NotFound();

            var currentRoles = await _userManager.GetRolesAsync(user);

            // Remove todas as roles atuais
            var removeResult = await _userManager.RemoveFromRolesAsync(user, currentRoles);
            if (!removeResult.Succeeded)
            {
                foreach (var error in removeResult.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
                return View(model);
            }

            // Adiciona as novas roles selecionadas
            if (model.SelectedRoles != null && model.SelectedRoles.Any())
            {
                var addResult = await _userManager.AddToRolesAsync(user, model.SelectedRoles);
                if (!addResult.Succeeded)
                {
                    foreach (var error in addResult.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }
                    return View(model);
                }
            }

            _logger.LogInformation($"Usuário {user.Email} teve suas roles alteradas por {User.Identity.Name}");
            TempData["Sucesso"] = $"Roles do usuário {user.UserName} atualizadas com sucesso!";

            return RedirectToAction(nameof(Index));
        }

        // Ativa ou desativa um usuário
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleActive(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
                return NotFound();

            user.IsActive = !user.IsActive;
            var result = await _userManager.UpdateAsync(user);

            if (result.Succeeded)
            {
                var status = user.IsActive ? "ativado" : "desativado";
                _logger.LogInformation($"Usuário {user.Email} foi {status} por {User.Identity.Name}");
                TempData["Sucesso"] = $"Usuário {user.UserName} foi {status} com sucesso!";
            }
            else
            {
                TempData["Erro"] = "Erro ao alterar status do usuário";
            }

            return RedirectToAction(nameof(Index));
        }

        // Exclui um usuário do sistema (não permite excluir o admin padrão)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
                return NotFound();

            // Impede exclusão do usuário administrador padrão
            if (user.UserName == "admin@sistema.com")
            {
                TempData["Erro"] = "Não é possível excluir o usuário administrador padrão.";
                return RedirectToAction(nameof(Index));
            }

            var result = await _userManager.DeleteAsync(user);

            if (result.Succeeded)
            {
                _logger.LogInformation($"Usuário {user.Email} foi excluído por {User.Identity.Name}");
                TempData["Sucesso"] = $"Usuário {user.UserName} excluído com sucesso!";
            }
            else
            {
                TempData["Erro"] = "Erro ao excluir usuário";
            }

            return RedirectToAction(nameof(Index));
        }

        // Reseta a senha de um usuário para o padrão "User@123"
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
                return NotFound();

            var newPassword = "User@123";
            var resetToken = await _userManager.GeneratePasswordResetTokenAsync(user);
            var result = await _userManager.ResetPasswordAsync(user, resetToken, newPassword);

            if (result.Succeeded)
            {
                _logger.LogInformation($"Senha do usuário {user.Email} foi resetada por {User.Identity.Name}");
                TempData["Sucesso"] = $"Senha do usuário {user.UserName} foi resetada para '{newPassword}'";
            }
            else
            {
                TempData["Erro"] = "Erro ao resetar senha";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}