using SistemaAereo.Models.Entities;

namespace SistemaAereo.Repositories.Interfaces
{
    // Interface específica para o repositório de aeronaves
    // Herda os métodos genéricos do IRepository
    public interface IAircraftRepository : IRepository<Aircraft>
    {
        // Obtém todas as aeronaves com seus respectivos voos carregados (Include)
        Task<IEnumerable<Aircraft>> GetAircraftsWithFlightsAsync();

        // Verifica se uma aeronave possui voos associados
        // Útil para impedir exclusão de aeronaves com voos
        Task<bool> HasFlightsAsync(int aircraftId);
    }
}