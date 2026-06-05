using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SistemaAereo.Models.Entities;
using SistemaAereo.Models.ViewModels;
using SistemaAereo.Services.Interfaces;

namespace SistemaAereo.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly IAvatarService _avatarService;
        private readonly ILogger<AccountController> _logger;

        public AccountController(
            UserManager<User> userManager,
            SignInManager<User> signInManager,
            IAvatarService avatarService,
            ILogger<AccountController> logger)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _avatarService = avatarService;
            _logger = logger;
        }

        // =============================================
        // MÉTODOS DE AUTENTICAÇÃO
        // =============================================

        // Exibe a página de login
        [AllowAnonymous]
        public IActionResult Login(string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        // Processa a tentativa de login do usuário
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;

            if (ModelState.IsValid)
            {
                // Tenta fazer login com as credenciais fornecidas
                var result = await _signInManager.PasswordSignInAsync(
                    model.Email,
                    model.Password,
                    model.RememberMe,
                    lockoutOnFailure: false);

                if (result.Succeeded)
                {
                    _logger.LogInformation($"Usuário {model.Email} logou com sucesso.");

                    // Verifica se o usuário está ativo
                    var user = await _userManager.FindByEmailAsync(model.Email);
                    if (user != null && !user.IsActive)
                    {
                        await _signInManager.SignOutAsync();
                        ModelState.AddModelError(string.Empty, "Usuário inativo. Contate o administrador.");
                        return View(model);
                    }

                    // Redireciona para a página solicitada ou para o Dashboard
                    if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                    {
                        return Redirect(returnUrl);
                    }

                    return RedirectToAction("Index", "Home");
                }

                // Usuário bloqueado por muitas tentativas
                if (result.IsLockedOut)
                {
                    _logger.LogWarning($"Usuário {model.Email} bloqueado.");
                    return RedirectToAction("Lockout");
                }

                ModelState.AddModelError(string.Empty, "Email ou senha inválidos.");
                return View(model);
            }

            return View(model);
        }

        // Exibe a página de registro de novo usuário
        [AllowAnonymous]
        public IActionResult Register()
        {
            return View();
        }

        // Processa o registro de um novo usuário
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                // Gera um avatar para o novo usuário baseado no nome
                var avatarUrl = _avatarService.GerarAvatarUrl(model.FullName ?? model.Email, 128);

                var user = new User
                {
                    UserName = model.Email,
                    Email = model.Email,
                    FullName = model.FullName,
                    IsActive = true,
                    RegistrationDate = DateTime.Now,
                    AvatarUrl = avatarUrl
                };

                var result = await _userManager.CreateAsync(user, model.Password);

                if (result.Succeeded)
                {
                    _logger.LogInformation($"Usuário {user.Email} criado com sucesso.");

                    // Adiciona o usuário à role padrão "User"
                    await _userManager.AddToRoleAsync(user, "User");

                    // Faz login automático após o registro
                    await _signInManager.SignInAsync(user, isPersistent: false);

                    return RedirectToAction("Index", "Home");
                }

                // Exibe os erros de validação
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            return View(model);
        }

        // Realiza o logout do usuário
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            _logger.LogInformation("Usuário fez logout.");
            return RedirectToAction("Login", "Account");
        }

        // Exibe página de acesso negado
        [AllowAnonymous]
        public IActionResult AccessDenied()
        {
            return View();
        }

        // Exibe página de conta bloqueada
        [AllowAnonymous]
        public IActionResult Lockout()
        {
            return View();
        }

        // =============================================
        // MÉTODOS DE PERFIL
        // =============================================

        // Exibe a página de perfil do usuário
        [Authorize]
        public async Task<IActionResult> Profile()
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                {
                    _logger.LogWarning("Usuário não encontrado no Profile");
                    return RedirectToAction("Login", "Account");
                }

                var model = new ProfileViewModel
                {
                    Email = user.Email,
                    FullName = user.FullName ?? string.Empty,
                    Phone = user.PhoneNumber ?? string.Empty,
                    RegistrationDate = user.RegistrationDate
                };

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao carregar perfil do usuário");
                TempData["Erro"] = "Erro ao carregar perfil. Tente novamente.";
                return RedirectToAction("Index", "Home");
            }
        }

        // Atualiza os dados do perfil do usuário
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Profile(ProfileViewModel model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var user = await _userManager.GetUserAsync(User);
                    if (user == null)
                    {
                        return NotFound();
                    }

                    user.FullName = model.FullName;
                    user.PhoneNumber = model.Phone;

                    var result = await _userManager.UpdateAsync(user);

                    if (result.Succeeded)
                    {
                        TempData["Sucesso"] = "Perfil atualizado com sucesso!";
                        return RedirectToAction("Profile");
                    }

                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }
                }

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao atualizar perfil");
                TempData["Erro"] = "Erro ao atualizar perfil. Tente novamente.";
                return View(model);
            }
        }

        // Exibe a página de alteração de senha
        [Authorize]
        public IActionResult ChangePassword()
        {
            return View();
        }

        // Processa a alteração da senha do usuário
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                {
                    return NotFound();
                }

                var result = await _userManager.ChangePasswordAsync(user, model.CurrentPassword, model.NewPassword);

                if (result.Succeeded)
                {
                    await _signInManager.RefreshSignInAsync(user);
                    TempData["Sucesso"] = "Senha alterada com sucesso!";
                    return RedirectToAction("Profile");
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            return View(model);
        }
    }
}