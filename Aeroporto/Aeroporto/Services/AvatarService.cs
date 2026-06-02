using SistemaAereo.Services.Interfaces;

namespace SistemaAereo.Services
{
    /// <summary>
    /// Serviço para geração de avatares usando a API DiceBear
    /// </summary>
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
            var seed = nome.ToLower()
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

            // Gerar URL com base no estilo selecionado
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
    }
}