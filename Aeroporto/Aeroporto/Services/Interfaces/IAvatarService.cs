namespace SistemaAereo.Services.Interfaces
{
    public interface IAvatarService
    {
        /// <summary>
        /// Gera URL de avatar baseado no nome do usuário
        /// </summary>
        string GerarAvatarUrl(string nome, int tamanho = 128, string estilo = null);

        /// <summary>
        /// Gera URL de avatar baseado no email
        /// </summary>
        string GerarAvatarPorEmail(string email, int tamanho = 128, string estilo = null);

        /// <summary>
        /// Baixa a imagem do avatar como array de bytes
        /// </summary>
        Task<byte[]> BaixarAvatarAsync(string nome, int tamanho = 128, string estilo = null);

        /// <summary>
        /// Obtém uma lista de estilos disponíveis para avatar
        /// </summary>
        List<string> ObterEstilosDisponiveis();

        /// <summary>
        /// Obtém uma lista de provedores disponíveis
        /// </summary>
        List<string> ObterProvedoresDisponiveis();

        /// <summary>
        /// Gera avatar completo com provedor e estilo
        /// </summary>
        string GerarAvatarCompleto(string nome, int tamanho = 128, string provedor = "dicebear", string estilo = null);

        /// <summary>
        /// Gera avatar usando UI Avatars (iniciais)
        /// </summary>
        string GerarAvatarUI(string nome, int tamanho = 128);

        /// <summary>
        /// Gera avatar usando MultiAvatar
        /// </summary>
        string GerarAvatarMulti(string nome, int tamanho = 128);
    }
}