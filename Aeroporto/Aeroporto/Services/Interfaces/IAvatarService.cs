namespace SistemaAereo.Services.Interfaces
{
    /// <summary>
    /// Interface para serviço de geração de avatares
    /// </summary>
    public interface IAvatarService
    {
        /// <summary>
        /// Gera URL de avatar baseado no nome do usuário
        /// </summary>
        /// <param name="nome">Nome do usuário</param>
        /// <param name="tamanho">Tamanho da imagem em pixels</param>
        /// <param name="estilo">Estilo do avatar (avataaars, bottts, identicon, initials, thumbs)</param>
        /// <returns>URL do avatar gerado</returns>
        string GerarAvatarUrl(string nome, int tamanho = 128, string estilo = null);

        /// <summary>
        /// Gera URL de avatar baseado no email
        /// </summary>
        /// <param name="email">Email do usuário</param>
        /// <param name="tamanho">Tamanho da imagem em pixels</param>
        /// <param name="estilo">Estilo do avatar</param>
        /// <returns>URL do avatar gerado</returns>
        string GerarAvatarPorEmail(string email, int tamanho = 128, string estilo = null);

        /// <summary>
        /// Baixa a imagem do avatar como array de bytes
        /// </summary>
        /// <param name="nome">Nome do usuário</param>
        /// <param name="tamanho">Tamanho da imagem</param>
        /// <param name="estilo">Estilo do avatar</param>
        /// <returns>Array de bytes da imagem</returns>
        Task<byte[]> BaixarAvatarAsync(string nome, int tamanho = 128, string estilo = null);

        /// <summary>
        /// Obtém uma lista de estilos disponíveis para avatar
        /// </summary>
        List<string> ObterEstilosDisponiveis();
    }
}