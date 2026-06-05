using SistemaAereo.Models.Entities;

namespace SistemaAereo.Repositories.Interfaces
{
    // Interface específica para o repositório de aeroportos
    // Herda os métodos genéricos do IRepository
    public interface IAirportRepository : IRepository<Airport>
    {
        // Verifica se um código IATA já existe no banco
        // excludeId: ID do aeroporto a ser ignorado (útil para edição)
        Task<bool> IATACodeExistsAsync(string iataCode, int? excludeId = null);

        // Verifica se um aeroporto possui voos associados (como origem ou destino)
        // Útil para impedir exclusão de aeroportos com voos
        Task<bool> HasFlightsAsync(int airportId);
    }
}