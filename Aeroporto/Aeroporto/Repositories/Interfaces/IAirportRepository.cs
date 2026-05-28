using SistemaAereo.Models.Entities;

namespace SistemaAereo.Repositories.Interfaces
{
    public interface IAirportRepository : IRepository<Airport>
    {
        Task<bool> IATACodeExistsAsync(string iataCode, int? excludeId = null);
        Task<bool> HasFlightsAsync(int airportId);
    }
}