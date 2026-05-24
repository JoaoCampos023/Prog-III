using SistemaAereo.Models;

namespace SistemaAereo.Repositories.Interfaces
{
    public interface IAeroportoRepository : IRepository<Aeroporto>
    {
        // =============================================
        // VALIDAÇÕES DE UNICIDADE
        // =============================================

        Task<bool> CodigoIATAExistsAsync(string codigoIATA, int? excludeId = null);

        // =============================================
        // VALIDAÇÕES DE DEPENDÊNCIA
        // =============================================

        Task<bool> HasVoosAsync(int aeroportoId);
    }
}