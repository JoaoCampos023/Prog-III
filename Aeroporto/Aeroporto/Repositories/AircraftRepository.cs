using Microsoft.EntityFrameworkCore;
using SistemaAereo.Data.Context;
using SistemaAereo.Models.Entities;
using SistemaAereo.Repositories.Interfaces;

namespace SistemaAereo.Repositories
{
    public class AircraftRepository : Repository<Aircraft>, IAircraftRepository
    {
        public AircraftRepository(AirportsContext context) : base(context) { }

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

        public async Task<bool> HasFlightsAsync(int aircraftId)
        {
            return await _context.Flights.AnyAsync(f => f.AircraftId == aircraftId);
        }
    }
}