using Microsoft.EntityFrameworkCore;
using SistemaAereo.Data.Context;
using SistemaAereo.Models.Entities;
using SistemaAereo.Repositories.Interfaces;

namespace SistemaAereo.Repositories
{
    // Implementação do repositório de aeronaves
    public class AircraftRepository : Repository<Aircraft>, IAircraftRepository
    {
        public AircraftRepository(AirportsContext context) : base(context) { }

        // Obtém todas as aeronaves com seus respectivos voos carregados (Include)
        public async Task<IEnumerable<Aircraft>> GetAircraftsWithFlightsAsync()
        {
            return await _dbSet
                .Include(a => a.Flights)
                    .ThenInclude(f => f.DepartureAirport)
                .Include(a => a.Flights)
                    .ThenInclude(f => f.ArrivalAirport)
                .OrderBy(a => a.AircraftType)
                .ToListAsync();
        }

        // Verifica se uma aeronave possui voos associados
        public async Task<bool> HasFlightsAsync(int aircraftId)
        {
            // Verifica se existe algum voo que utiliza esta aeronave
            return await _context.Flights.AnyAsync(f => f.AircraftId == aircraftId);
        }
    }
}