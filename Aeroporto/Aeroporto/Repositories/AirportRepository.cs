using Microsoft.EntityFrameworkCore;
using SistemaAereo.Data.Context;
using SistemaAereo.Models.Entities;
using SistemaAereo.Repositories.Interfaces;

namespace SistemaAereo.Repositories
{
    public class AirportRepository : Repository<Airport>, IAirportRepository
    {
        public AirportRepository(AirportsContext context) : base(context) { }

        public async Task<bool> IATACodeExistsAsync(string iataCode, int? excludeId = null)
        {
            if (excludeId.HasValue)
                return await _dbSet.AnyAsync(a =>
                    a.IATACode == iataCode &&
                    a.AirportId != excludeId.Value);

            return await _dbSet.AnyAsync(a => a.IATACode == iataCode);
        }

        public async Task<bool> HasFlightsAsync(int airportId)
        {
            return await _context.Flights.AnyAsync(f =>
                f.DepartureAirportId == airportId ||
                f.ArrivalAirportId == airportId);
        }
    }
}