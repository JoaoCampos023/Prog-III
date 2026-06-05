using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SistemaAereo.Models.Entities;
using SistemaAereo.Services.Interfaces;

namespace SistemaAereo.Controllers.Api
{
    // API para gerenciamento de avatares dos usuários
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class AvatarController : ControllerBase
    {
        private readonly IAvatarService _avatarService;
        private readonly UserManager<User> _userManager;
        private readonly ILogger<AvatarController> _logger;

        public AvatarController(
            IAvatarService avatarService,
            UserManager<User> userManager,
            ILogger<AvatarController> logger)
        {
            _avatarService = avatarService;
            _userManager = userManager;
            _logger = logger;
        }

        // Gera um avatar placeholder (fallback)
        [HttpGet("placeholder")]
        [AllowAnonymous]
        public IActionResult GetPlaceholderAvatar(int size = 128)
        {
            // SVG simples de placeholder
            var svg = $@"<svg width='{size}' height='{size}' viewBox='0 0 100 100' xmlns='http://www.w3.org/2000/svg'>
                <circle cx='50' cy='50' r='50' fill='#667eea'/>
                <circle cx='50' cy='35' r='15' fill='white'/>
                <path d='M20 75 Q50 85 80 75' stroke='white' stroke-width='5' fill='none' stroke-linecap='round'/>
                <text x='50' y='90' text-anchor='middle' fill='white' font-size='12' font-family='Arial'>?</text>
            </svg>";

            var bytes = System.Text.Encoding.UTF8.GetBytes(svg);
            return File(bytes, "image/svg+xml");
        }

        // Obtém o avatar do usuário atual
        [HttpGet("current")]
        public async Task<IActionResult> GetCurrentAvatar(int size = 128)
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                    return Redirect("/api/avatar/placeholder?size=" + size);

                var avatarUrl = user.AvatarUrl;

                // Se não tiver avatar, gera um novo
                if (string.IsNullOrEmpty(avatarUrl))
                {
                    var timestamp = DateTime.Now.Ticks;
                    avatarUrl = _avatarService.GerarAvatarUrl(user.FullName ?? user.Email, size);
                    avatarUrl = $"{avatarUrl}&t={timestamp}";

                    user.AvatarUrl = avatarUrl;
                    await _userManager.UpdateAsync(user);
                }

                // Impede cache da imagem
                Response.Headers.Add("Cache-Control", "no-cache, no-store, must-revalidate");
                Response.Headers.Add("Pragma", "no-cache");
                Response.Headers.Add("Expires", "0");

                using var httpClient = new HttpClient();
                var imageBytes = await httpClient.GetByteArrayAsync(avatarUrl);

                return File(imageBytes, "image/svg+xml");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao obter avatar");
                return Redirect("/api/avatar/placeholder?size=" + size);
            }
        }

        // Obtém avatar para um usuário específico (apenas Admin)
        [HttpGet("user/{userId}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetUserAvatar(string userId, int size = 128)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                    return NotFound();

                var avatarUrl = user.AvatarUrl ?? _avatarService.GerarAvatarUrl(user.FullName ?? user.Email, size);

                using var httpClient = new HttpClient();
                var imageBytes = await httpClient.GetByteArrayAsync(avatarUrl);

                return File(imageBytes, "image/svg+xml");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao obter avatar do usuário {UserId}", userId);
                return StatusCode(500);
            }
        }

        // Retorna a lista de estilos disponíveis para avatar
        [HttpGet("styles")]
        public IActionResult GetStyles()
        {
            var styles = _avatarService.ObterEstilosDisponiveis();
            return Ok(new { success = true, data = styles });
        }

        // Retorna a lista de provedores disponíveis
        [HttpGet("providers")]
        public IActionResult GetProviders()
        {
            var providers = _avatarService.ObterProvedoresDisponiveis();
            return Ok(new { success = true, data = providers });
        }

        // Gera preview de um estilo específico
        [HttpGet("preview/{style}")]
        public async Task<IActionResult> GetStylePreview(string style, int size = 80)
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                var nome = user?.FullName ?? user?.Email ?? "usuario";

                var avatarUrl = _avatarService.GerarAvatarUrl(nome, size, style);

                using var httpClient = new HttpClient();
                var imageBytes = await httpClient.GetByteArrayAsync(avatarUrl);

                return File(imageBytes, "image/svg+xml");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao gerar preview do estilo {Style}", style);
                return StatusCode(500);
            }
        }

        // Gera avatar usando provedor específico
        [HttpGet("provider/{provider}")]
        public async Task<IActionResult> GetAvatarByProvider(string provider, int size = 128, string style = null)
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                    return NotFound();

                var avatarUrl = _avatarService.GerarAvatarCompleto(user.FullName ?? user.Email, size, provider, style);

                using var httpClient = new HttpClient();
                var imageBytes = await httpClient.GetByteArrayAsync(avatarUrl);

                return File(imageBytes, "image/svg+xml");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao gerar avatar com provedor {Provider}", provider);
                return StatusCode(500);
            }
        }

        // Gera avatar UI (iniciais do nome)
        [HttpGet("ui")]
        public async Task<IActionResult> GetUIAvatar(int size = 128)
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                    return NotFound();

                var avatarUrl = _avatarService.GerarAvatarUI(user.FullName ?? user.Email, size);

                using var httpClient = new HttpClient();
                var imageBytes = await httpClient.GetByteArrayAsync(avatarUrl);

                return File(imageBytes, "image/svg+xml");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao gerar UI avatar");
                return StatusCode(500);
            }
        }

        // Altera o estilo do avatar do usuário
        [HttpPost("change-style")]
        public async Task<IActionResult> ChangeAvatarStyle([FromBody] ChangeStyleRequest request)
        {
            try
            {
                if (request == null || string.IsNullOrEmpty(request.Style))
                {
                    return BadRequest(new { success = false, message = "Estilo não informado" });
                }

                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                    return NotFound(new { success = false, message = "Usuário não encontrado" });

                var estilosDisponiveis = _avatarService.ObterEstilosDisponiveis();
                var provedoresDisponiveis = _avatarService.ObterProvedoresDisponiveis();

                // Verifica se o estilo/provedor é válido
                bool isValid = estilosDisponiveis.Contains(request.Style) || provedoresDisponiveis.Contains(request.Style);

                if (!isValid)
                {
                    return BadRequest(new { success = false, message = "Estilo de avatar inválido" });
                }

                var timestamp = DateTime.Now.Ticks;
                string newAvatarUrl;

                if (request.Style == "ui-avatars")
                {
                    newAvatarUrl = _avatarService.GerarAvatarUI(user.FullName ?? user.Email, 128);
                }
                else if (request.Style == "multiavatar")
                {
                    newAvatarUrl = _avatarService.GerarAvatarMulti(user.FullName ?? user.Email, 128);
                }
                else
                {
                    newAvatarUrl = _avatarService.GerarAvatarUrl(user.FullName ?? user.Email, 128, request.Style);
                }

                newAvatarUrl = $"{newAvatarUrl}&t={timestamp}";

                user.AvatarUrl = newAvatarUrl;
                var result = await _userManager.UpdateAsync(user);

                if (result.Succeeded)
                {
                    _logger.LogInformation($"Estilo do avatar alterado para {request.Style} para o usuário {user.Email}");
                    return Ok(new { success = true, message = $"Estilo alterado para {request.Style}" });
                }

                return BadRequest(new { success = false, message = "Erro ao alterar estilo" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao alterar estilo do avatar");
                return StatusCode(500, new { success = false, message = "Erro interno ao alterar estilo" });
            }
        }

        // Regenera o avatar do usuário atual (novo seed aleatório)
        [HttpPost("regenerate")]
        public async Task<IActionResult> RegenerateAvatar()
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                    return NotFound(new { success = false, message = "Usuário não encontrado" });

                var timestamp = DateTime.Now.Ticks;
                var newAvatarUrl = _avatarService.GerarAvatarUrl(user.FullName ?? user.Email, 128);
                newAvatarUrl = $"{newAvatarUrl}&t={timestamp}";

                user.AvatarUrl = newAvatarUrl;
                var result = await _userManager.UpdateAsync(user);

                if (result.Succeeded)
                {
                    _logger.LogInformation($"Avatar regenerado para o usuário {user.Email}");
                    return Ok(new { success = true, message = "Avatar regenerado com sucesso" });
                }

                return BadRequest(new { success = false, message = "Erro ao regenerar avatar" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao regenerar avatar");
                return StatusCode(500, new { success = false, message = "Erro interno ao regenerar avatar" });
            }
        }

        // Baixa o avatar do usuário atual como arquivo SVG
        [HttpGet("download")]
        public async Task<IActionResult> DownloadAvatar(int size = 256)
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                    return NotFound();

                var avatarUrl = user.AvatarUrl;
                if (string.IsNullOrEmpty(avatarUrl))
                {
                    avatarUrl = _avatarService.GerarAvatarUrl(user.FullName ?? user.Email, size);
                }

                using var httpClient = new HttpClient();
                var imageBytes = await httpClient.GetByteArrayAsync(avatarUrl);

                return File(imageBytes, "image/svg+xml", $"avatar_{user.UserName}.svg");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao baixar avatar");
                return StatusCode(500);
            }
        }
    }

    // Classe auxiliar para requisição de mudança de estilo
    public class ChangeStyleRequest
    {
        public string Style { get; set; }
    }
}