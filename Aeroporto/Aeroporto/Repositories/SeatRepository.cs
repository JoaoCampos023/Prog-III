using Microsoft.EntityFrameworkCore;
using SistemaAereo.Data.Context;
using SistemaAereo.Models.Entities;
using SistemaAereo.Repositories.Interfaces;

namespace SistemaAereo.Repositories
{
    // Implementação do repositório de poltronas
    public class SeatRepository : Repository<Seat>, ISeatRepository
    {
        public SeatRepository(AirportsContext context) : base(context) { }

        // Obtém todas as poltronas de um voo
        public async Task<IEnumerable<Seat>> GetSeatsByFlightAsync(int flightId)
        {
            return await _dbSet
                .AsNoTracking()
                .Include(s => s.Flight)
                .Where(s => s.FlightId == flightId)
                .OrderBy(s => s.SeatNumber)
                .ToListAsync();
        }

        // Obtém apenas as poltronas disponíveis de um voo
        public async Task<IEnumerable<Seat>> GetAvailableSeatsByFlightAsync(int flightId)
        {
            return await _dbSet
                .AsNoTracking()
                .Include(s => s.Flight)
                .Where(s => s.FlightId == flightId && s.IsAvailable)
                .OrderBy(s => s.SeatNumber)
                .ToListAsync();
        }

        // Obtém uma poltrona com os dados do voo associado
        public async Task<Seat> GetSeatWithFlightAsync(int id)
        {
            return await _dbSet
                .Include(s => s.Flight)
                .FirstOrDefaultAsync(s => s.SeatId == id);
        }

        // Verifica se um número de poltrona já existe em um voo
        public async Task<bool> SeatNumberExistsInFlightAsync(int flightId, string seatNumber)
        {
            return await _dbSet.AnyAsync(s =>
                s.FlightId == flightId && s.SeatNumber == seatNumber);
        }

        // Total de poltronas disponíveis em um voo
        public async Task<int> GetTotalAvailableSeatsByFlightAsync(int flightId)
        {
            return await _dbSet.CountAsync(s => s.FlightId == flightId && s.IsAvailable);
        }

        // Total de poltronas de um voo (disponíveis + ocupadas)
        public async Task<int> GetTotalSeatsByFlightAsync(int flightId)
        {
            return await _dbSet.CountAsync(s => s.FlightId == flightId);
        }
    }
}