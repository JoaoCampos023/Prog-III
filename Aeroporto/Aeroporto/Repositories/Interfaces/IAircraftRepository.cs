using SistemaAereo.Models.Entities;

namespace SistemaAereo.Repositories.Interfaces
{
    public interface IAircraftRepository : IRepository<Aircraft>
    {
        Task<IEnumerable<Aircraft>> GetAircraftsWithFlightsAsync();
        Task<bool> HasFlightsAsync(int aircraftId);
    }
}