using SistemaAereo.Models;

namespace SistemaAereo.Repositories.Interfaces
{
    public interface IClientePreferencialRepository : IRepository<ClientePreferencial>
    {
        // =============================================
        // CONSULTAS DE CLIENTES
        // =============================================

        Task<IEnumerable<ClientePreferencial>> GetClientesAtivosAsync();
        Task<IEnumerable<ClientePreferencial>> GetClientesInativosAsync();
        Task<IEnumerable<ClientePreferencial>> GetAllClientesAsync();
        Task<int> GetTotalClientesAtivosAsync();
        Task<int> GetTotalClientesInativosAsync();

        // =============================================
        // VALIDAÇÕES DE UNICIDADE
        // =============================================

        Task<bool> EmailExistsAsync(string email, int? excludeId = null);
        Task<bool> CPFExistsAsync(string cpf, int? excludeId = null);
    }
}