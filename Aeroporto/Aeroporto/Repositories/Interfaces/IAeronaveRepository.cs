using SistemaAereo.Models.Entities;

namespace SistemaAereo.Repositories.Interfaces
{
    public interface IAeronaveRepository : IRepository<Aeronave>
    {
        // =============================================
        // CONSULTAS DE AERONAVES
        // =============================================

        Task<IEnumerable<Aeronave>> GetAeronavesComVoosAsync();

        // =============================================
        // VALIDAÇÕES DE DEPENDÊNCIA
        // =============================================

        Task<bool> HasVoosAsync(int aeronaveId);
    }
}