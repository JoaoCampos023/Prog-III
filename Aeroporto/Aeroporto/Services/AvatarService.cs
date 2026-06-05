using SistemaAereo.Services.Interfaces;

namespace SistemaAereo.Services
{
    // Implementação do serviço de geração de avatares
    // Utiliza a API DiceBear (https://dicebear.com/)
    public class AvatarService : IAvatarService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<AvatarService> _logger;

        // Estilos disponíveis na API DiceBear
        private readonly List<string> _estilosDisponiveis = new List<string>
        {
            "avataaars",      // Estilo padrão de pessoas
            "bottts",         // Estilo de robôs
            "identicon",      // Estilo geométrico
            "initials",       // Iniciais do nome
            "thumbs",         // Estilo emoji/polegares
            "adventurer",     // Aventureiro
            "micah",          // Estilo minimalista
            "open-peeps",     // Pessoas diversas
            "pixel-art",      // Arte em pixel
            "lorelei"         // Estilo cartoon
        };

        // Provedores alternativos de avatar
        private readonly List<string> _provedoresDisponiveis = new List<string>
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

        // =============================================
        // GERAÇÃO DE URLs DE AVATAR
        // =============================================

        // Gera URL de avatar baseado no nome do usuário
        public string GerarAvatarUrl(string nome, int tamanho = 128, string estilo = null)
        {
            if (string.IsNullOrEmpty(nome))
                nome = "user";

            // Escolhe estilo aleatório se não especificado (baseado no hash do nome)
            if (string.IsNullOrEmpty(estilo))
            {
                estilo = _estilosDisponiveis[new Random(nome.GetHashCode()).Next(_estilosDisponiveis.Count)];
            }

            // Limpa o nome para usar como seed (remove acentos e caracteres especiais)
            var seed = LimparSeed(nome);

            // Gera URL com base no estilo selecionado
            string url;
            switch (estilo)
            {
                case "initials":
                    // Para estilo de iniciais, usar as primeiras letras do nome
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

        // Gera URL de avatar baseado no email
        public string GerarAvatarPorEmail(string email, int tamanho = 128, string estilo = null)
        {
            if (string.IsNullOrEmpty(email))
                email = "user@example.com";

            var nome = email.Split('@')[0];
            return GerarAvatarUrl(nome, tamanho, estilo);
        }

        // Gera URL de avatar usando UI Avatars (iniciais com fundo colorido)
        public string GerarAvatarUI(string nome, int tamanho = 128)
        {
            if (string.IsNullOrEmpty(nome))
                nome = "U";

            var iniciais = ObterIniciais(nome);
            var cor = GerarCorDoNome(nome);

            return $"https://ui-avatars.com/api/?name={iniciais}&size={tamanho}&background={cor}&color=fff&bold=true&length=2";
        }

        // Gera URL de avatar usando MultiAvatar
        public string GerarAvatarMulti(string nome, int tamanho = 128)
        {
            if (string.IsNullOrEmpty(nome))
                nome = "user";

            var seed = LimparSeed(nome);
            return $"https://api.multiavatar.com/{seed}.svg?size={tamanho}";
        }

        // Gera avatar completo com provedor e estilo específicos
        public string GerarAvatarCompleto(string nome, int tamanho = 128, string provedor = "dicebear", string estilo = null)
        {
            return provedor?.ToLower() switch
            {
                "ui-avatars" => GerarAvatarUI(nome, tamanho),
                "multiavatar" => GerarAvatarMulti(nome, tamanho),
                _ => GerarAvatarUrl(nome, tamanho, estilo)
            };
        }

        // =============================================
        // DOWNLOAD DE AVATAR
        // =============================================

        // Baixa a imagem do avatar como array de bytes
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

        // =============================================
        // CONSULTAS
        // =============================================

        // Obtém uma lista de estilos disponíveis para avatar
        public List<string> ObterEstilosDisponiveis()
        {
            return _estilosDisponiveis;
        }

        // Obtém uma lista de provedores disponíveis
        public List<string> ObterProvedoresDisponiveis()
        {
            return _provedoresDisponiveis;
        }

        // =============================================
        // MÉTODOS PRIVADOS AUXILIARES
        // =============================================

        // Obtém as iniciais de um nome (ex: "João Silva" -> "JS")
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

        // Gera uma cor baseada no nome (para o UI Avatars)
        private string GerarCorDoNome(string nome)
        {
            var cores = new[] { "3b82f6", "ef4444", "10b981", "f59e0b", "8b5cf6", "06b6d4", "ec4899", "14b8a6" };
            var hash = nome.GetHashCode();
            var index = Math.Abs(hash) % cores.Length;
            return cores[index];
        }

        // Limpa o seed para URL (remove acentos e caracteres especiais)
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