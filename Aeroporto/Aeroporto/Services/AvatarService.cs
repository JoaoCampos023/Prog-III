using SistemaAereo.Services.Interfaces;

namespace SistemaAereo.Services
{
    /// <summary>
    /// Serviço para geração de avatares usando várias APIs
    /// </summary>
    public class AvatarService : IAvatarService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<AvatarService> _logger;

        // Estilos disponíveis na API DiceBear
        private readonly List<string> _estilosDisponiveis = new List<string>
        {
            "avataaars",      // Pessoas (padrão)
            "bottts",         // Robôs
            "identicon",      // Geométrico
            "initials",       // Iniciais do nome
            "thumbs",         // Emoji/polegares
            "adventurer",     // Aventureiro
            "micah",          // Minimalista
            "open-peeps",     // Pessoas diversas
            "pixel-art",      // Arte em pixel
            "lorelei",        // Cartoon
            "fun-emoji",      // Emojis divertidos
            "glass",          // Estilo vidro
            "croodles",       // Desenho animado
            "miniavs",        // Minimalista
            "big-ears",       // Orelhas grandes
            "big-smile"       // Sorriso grande
        };

        // Provedores alternativos de avatar
        private readonly List<string> _provedores = new List<string>
        {
            "dicebear",   // DiceBear (padrão)
            "ui-avatars", // UI Avatars (iniciais)
            "multiavatar" // MultiAvatar
        };

        public AvatarService(HttpClient httpClient, ILogger<AvatarService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        /// <summary>
        /// Gera URL de avatar baseado no nome do usuário
        /// </summary>
        public string GerarAvatarUrl(string nome, int tamanho = 128, string estilo = null)
        {
            if (string.IsNullOrEmpty(nome))
                nome = "user";

            // Escolher estilo aleatório se não especificado
            if (string.IsNullOrEmpty(estilo))
            {
                estilo = _estilosDisponiveis[new Random(nome.GetHashCode()).Next(_estilosDisponiveis.Count)];
            }

            // Limpar o nome para usar como seed
            var seed = LimparSeed(nome);

            // Gerar URL com base no estilo selecionado
            string url;
            switch (estilo)
            {
                case "initials":
                    var iniciais = ObterIniciais(nome);
                    url = $"https://api.dicebear.com/9.x/initials/svg?seed={iniciais}&size={tamanho}&backgroundColor=b6e3f4&radius=50";
                    break;
                case "identicon":
                    url = $"https://api.dicebear.com/9.x/identicon/svg?seed={seed}&size={tamanho}&backgroundColor=b6e3f4&radius=50";
                    break;
                case "bottts":
                    url = $"https://api.dicebear.com/9.x/bottts/svg?seed={seed}&size={tamanho}&backgroundColor=b6e3f4&radius=50";
                    break;
                default:
                    url = $"https://api.dicebear.com/9.x/{estilo}/svg?seed={seed}&size={tamanho}&backgroundColor=b6e3f4&radius=50";
                    break;
            }

            return url;
        }

        /// <summary>
        /// Gera URL de avatar usando UI Avatars (iniciais com fundo)
        /// </summary>
        public string GerarAvatarUI(string nome, int tamanho = 128)
        {
            if (string.IsNullOrEmpty(nome))
                nome = "U";

            var iniciais = ObterIniciais(nome);
            var cor = GerarCorDoNome(nome);

            return $"https://ui-avatars.com/api/?name={iniciais}&size={tamanho}&background={cor}&color=fff&bold=true&length=2";
        }

        /// <summary>
        /// Gera URL de avatar usando MultiAvatar
        /// </summary>
        public string GerarAvatarMulti(string nome, int tamanho = 128)
        {
            if (string.IsNullOrEmpty(nome))
                nome = "user";

            var seed = LimparSeed(nome);
            return $"https://api.multiavatar.com/{seed}.svg?size={tamanho}";
        }

        /// <summary>
        /// Gera URL de avatar baseado no email
        /// </summary>
        public string GerarAvatarPorEmail(string email, int tamanho = 128, string estilo = null)
        {
            if (string.IsNullOrEmpty(email))
                email = "user@example.com";

            var nome = email.Split('@')[0];
            return GerarAvatarUrl(nome, tamanho, estilo);
        }

        /// <summary>
        /// Baixa a imagem do avatar como array de bytes
        /// </summary>
        public async Task<byte[]> BaixarAvatarAsync(string nome, int tamanho = 128, string estilo = null)
        {
            try
            {
                var url = GerarAvatarUrl(nome, tamanho, estilo);
                var response = await _httpClient.GetAsync(url);

                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadAsByteArrayAsync();
                }

                _logger.LogWarning("Falha ao baixar avatar para {Nome}. Status: {StatusCode}", nome, response.StatusCode);
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao baixar avatar para {Nome}", nome);
                return null;
            }
        }

        /// <summary>
        /// Obtém uma lista de estilos disponíveis para avatar
        /// </summary>
        public List<string> ObterEstilosDisponiveis()
        {
            return _estilosDisponiveis;
        }

        /// <summary>
        /// Obtém uma lista de provedores disponíveis
        /// </summary>
        public List<string> ObterProvedoresDisponiveis()
        {
            return _provedores;
        }

        /// <summary>
        /// Gera avatar completo com provedor e estilo
        /// </summary>
        public string GerarAvatarCompleto(string nome, int tamanho = 128, string provedor = "dicebear", string estilo = null)
        {
            return provedor?.ToLower() switch
            {
                "ui-avatars" => GerarAvatarUI(nome, tamanho),
                "multiavatar" => GerarAvatarMulti(nome, tamanho),
                _ => GerarAvatarUrl(nome, tamanho, estilo)
            };
        }

        /// <summary>
        /// Obtém as iniciais de um nome
        /// </summary>
        private string ObterIniciais(string nome)
        {
            if (string.IsNullOrEmpty(nome)) return "U";

            var partes = nome.Trim().Split(' ');
            if (partes.Length == 1)
                return partes[0].Substring(0, Math.Min(2, partes[0].Length)).ToUpper();

            var iniciais = "";
            for (int i = 0; i < Math.Min(2, partes.Length); i++)
            {
                if (!string.IsNullOrEmpty(partes[i]))
                    iniciais += partes[i][0];
            }
            return iniciais.ToUpper();
        }

        /// <summary>
        /// Gera uma cor baseada no nome
        /// </summary>
        private string GerarCorDoNome(string nome)
        {
            var cores = new[] { "3b82f6", "ef4444", "10b981", "f59e0b", "8b5cf6", "06b6d4", "ec4899", "14b8a6" };
            var hash = nome.GetHashCode();
            var index = Math.Abs(hash) % cores.Length;
            return cores[index];
        }

        /// <summary>
        /// Limpa o seed para URL
        /// </summary>
        private string LimparSeed(string nome)
        {
            return nome.ToLower()
                .Replace(" ", "")
                .Replace("@", "")
                .Replace(".", "")
                .Replace("-", "")
                .Replace("_", "")
                .Replace("ç", "c")
                .Replace("ã", "a")
                .Replace("á", "a")
                .Replace("é", "e")
                .Replace("í", "i")
                .Replace("ó", "o")
                .Replace("ú", "u");
        }
    }
}