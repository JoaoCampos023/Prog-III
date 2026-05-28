using SistemaAereo.Models.Entities;

namespace SistemaAereo.Repositories.Interfaces
{
    public interface ISeatRepository : IRepository<Seat>
    {
        Task<IEnumerable<Seat>> GetSeatsByFlightAsync(int flightId);
        Task<IEnumerable<Seat>> GetAvailableSeatsByFlightAsync(int flightId);
        Task<Seat> GetSeatWithFlightAsync(int id);
        Task<bool> SeatNumberExistsInFlightAsync(int flightId, string seatNumber);
        Task<int> GetTotalAvailableSeatsByFlightAsync(int flightId);
        Task<int> GetTotalSeatsByFlightAsync(int flightId);
    }
}