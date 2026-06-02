using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SistemaAereo.Models.Entities;
using SistemaAereo.Services.Interfaces;

namespace SistemaAereo.Controllers.Api
{
    /// <summary>
    /// API para gerenciamento de avatares dos usuários
    /// </summary>
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

        /// <summary>
        /// Obtém o avatar do usuário atual
        /// </summary>
        /// <param name="size">Tamanho da imagem (padrão: 128)</param>
        /// <returns>Imagem do avatar em SVG</returns>
        [HttpGet("current")]
        public async Task<IActionResult> GetCurrentAvatar(int size = 128)
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                    return NotFound(new { success = false, message = "Usuário não encontrado" });

                // Verificar se o usuário tem avatar salvo
                var avatarUrl = user.AvatarUrl;

                // Se não tiver avatar salvo, gerar um novo baseado no nome
                if (string.IsNullOrEmpty(avatarUrl))
                {
                    var timestamp = DateTime.Now.Ticks;
                    avatarUrl = _avatarService.GerarAvatarUrl(user.FullName ?? user.Email, size);
                    avatarUrl = $"{avatarUrl}&t={timestamp}";

                    user.AvatarUrl = avatarUrl;
                    await _userManager.UpdateAsync(user);
                }

                // Configurar cabeçalhos para evitar cache
                Response.Headers.Add("Cache-Control", "no-cache, no-store, must-revalidate");
                Response.Headers.Add("Pragma", "no-cache");
                Response.Headers.Add("Expires", "0");

                // Baixar a imagem
                using var httpClient = new HttpClient();
                var imageBytes = await httpClient.GetByteArrayAsync(avatarUrl);

                return File(imageBytes, "image/svg+xml");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao obter avatar do usuário");
                return StatusCode(500, new { success = false, message = "Erro ao obter avatar" });
            }
        }

        /// <summary>
        /// Obtém avatar para um usuário específico (para administradores)
        /// </summary>
        /// <param name="userId">ID do usuário</param>
        /// <param name="size">Tamanho da imagem</param>
        [HttpGet("user/{userId}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetUserAvatar(string userId, int size = 128)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                    return NotFound(new { success = false, message = "Usuário não encontrado" });

                var avatarUrl = user.AvatarUrl ?? _avatarService.GerarAvatarUrl(user.FullName ?? user.Email, size);

                using var httpClient = new HttpClient();
                var imageBytes = await httpClient.GetByteArrayAsync(avatarUrl);

                return File(imageBytes, "image/svg+xml");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao obter avatar do usuário {UserId}", userId);
                return StatusCode(500, new { success = false, message = "Erro ao obter avatar" });
            }
        }

        /// <summary>
        /// Obtém a lista de estilos disponíveis
        /// </summary>
        [HttpGet("styles")]
        public IActionResult GetStyles()
        {
            var styles = _avatarService.ObterEstilosDisponiveis();
            return Ok(new { success = true, data = styles });
        }

        /// <summary>
        /// Gera preview de um estilo específico
        /// </summary>
        /// <param name="style">Nome do estilo</param>
        /// <param name="size">Tamanho da imagem</param>
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
                return StatusCode(500, new { success = false, message = "Erro ao gerar preview" });
            }
        }

        /// <summary>
        /// Altera o estilo do avatar do usuário
        /// </summary>
        /// <param name="style">Nome do estilo</param>
        [HttpPost("change-style")]
        public async Task<IActionResult> ChangeAvatarStyle([FromBody] ChangeStyleRequest request)
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                    return NotFound(new { success = false, message = "Usuário não encontrado" });

                var estilosDisponiveis = _avatarService.ObterEstilosDisponiveis();
                if (!estilosDisponiveis.Contains(request.Style))
                {
                    return BadRequest(new { success = false, message = "Estilo de avatar inválido" });
                }

                var timestamp = DateTime.Now.Ticks;
                var newAvatarUrl = _avatarService.GerarAvatarUrl(user.FullName ?? user.Email, 128, request.Style);
                newAvatarUrl = $"{newAvatarUrl}&t={timestamp}";

                user.AvatarUrl = newAvatarUrl;
                var result = await _userManager.UpdateAsync(user);

                if (result.Succeeded)
                {
                    return Ok(new { success = true, message = $"Estilo alterado para {request.Style}" });
                }

                return BadRequest(new { success = false, message = "Erro ao alterar estilo" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao alterar estilo do avatar");
                return StatusCode(500, new { success = false, message = "Erro ao alterar estilo" });
            }
        }

        /// <summary>
        /// Regenera o avatar do usuário atual
        /// </summary>
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
                    return Ok(new { success = true, message = "Avatar regenerado com sucesso" });
                }

                return BadRequest(new { success = false, message = "Erro ao regenerar avatar" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao regenerar avatar");
                return StatusCode(500, new { success = false, message = "Erro ao regenerar avatar" });
            }
        }

        /// <summary>
        /// Baixa o avatar do usuário atual
        /// </summary>
        [HttpGet("download")]
        public async Task<IActionResult> DownloadAvatar(int size = 256)
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                    return NotFound(new { success = false, message = "Usuário não encontrado" });

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
                return StatusCode(500, new { success = false, message = "Erro ao baixar avatar" });
            }
        }
    }

    /// <summary>
    /// Request para alterar estilo do avatar
    /// </summary>
    public class ChangeStyleRequest
    {
        public string Style { get; set; }
    }
}