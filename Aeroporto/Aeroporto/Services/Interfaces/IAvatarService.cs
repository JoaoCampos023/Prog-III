namespace SistemaAereo.Services.Interfaces
{
    // Interface para o serviço de geração de avatares
    // Utiliza a API DiceBear para criar avatares personalizados
    public interface IAvatarService
    {
        // =============================================
        // GERAÇÃO DE URLs DE AVATAR
        // =============================================

        // Gera URL de avatar baseado no nome do usuário
        // Permite escolher tamanho e estilo do avatar
        // Parâmetros:
        //   - nome: Nome do usuário (usado como seed)
        //   - tamanho: Tamanho da imagem em pixels (padrão 128)
        //   - estilo: Estilo do avatar (avataaars, bottts, identicon, etc.)
        string GerarAvatarUrl(string nome, int tamanho = 128, string estilo = null);

        // Gera URL de avatar baseado no email do usuário
        // Extrai o nome da parte antes do @ e usa como seed
        string GerarAvatarPorEmail(string email, int tamanho = 128, string estilo = null);

        // Gera URL de avatar usando UI Avatars (iniciais com fundo colorido)
        // Cria um avatar simples baseado nas iniciais do nome
        string GerarAvatarUI(string nome, int tamanho = 128);

        // Gera URL de avatar usando MultiAvatar
        // Avatar único e diferenciado
        string GerarAvatarMulti(string nome, int tamanho = 128);

        // Gera avatar completo com provedor e estilo específicos
        // Permite escolher o provedor (dicebear, ui-avatars, multiavatar)
        string GerarAvatarCompleto(string nome, int tamanho = 128, string provedor = "dicebear", string estilo = null);

        // =============================================
        // DOWNLOAD DE AVATAR
        // =============================================

        // Baixa a imagem do avatar como array de bytes
        // Útil para salvar a imagem localmente
        Task<byte[]> BaixarAvatarAsync(string nome, int tamanho = 128, string estilo = null);

        // =============================================
        // CONSULTAS
        // =============================================

        // Obtém uma lista de estilos disponíveis para avatar (DiceBear)
        List<string> ObterEstilosDisponiveis();

        // Obtém uma lista de provedores disponíveis
        List<string> ObterProvedoresDisponiveis();
    }
}