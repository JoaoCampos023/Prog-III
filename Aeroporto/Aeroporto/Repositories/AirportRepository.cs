using Microsoft.EntityFrameworkCore;
using SistemaAereo.Data.Context;
using SistemaAereo.Models.Entities;
using SistemaAereo.Repositories.Interfaces;

namespace SistemaAereo.Repositories
{
    // Implementação do repositório de aeroportos
    public class AirportRepository : Repository<Airport>, IAirportRepository
    {
        public AirportRepository(AirportsContext context) : base(context) { }

        // Verifica se um código IATA já existe no banco
        public async Task<bool> IATACodeExistsAsync(string iataCode, int? excludeId = null)
        {
            // Se excludeId foi informado, ignora o aeroporto com esse ID (útil para edição)
            if (excludeId.HasValue)
                return await _dbSet.AnyAsync(a =>
                    a.IATACode == iataCode &&
                    a.AirportId != excludeId.Value);

            return await _dbSet.AnyAsync(a => a.IATACode == iataCode);
        }

        // Verifica se um aeroporto possui voos associados (como origem ou destino)
        public async Task<bool> HasFlightsAsync(int airportId)
        {
            // Verifica se existe algum voo com este aeroporto como origem OU destino
            return await _context.Flights.AnyAsync(f =>
                f.DepartureAirportId == airportId ||
                f.ArrivalAirportId == airportId);
        }
    }
}