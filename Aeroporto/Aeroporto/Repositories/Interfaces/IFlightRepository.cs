using SistemaAereo.Models.Entities;
using System.Linq.Expressions;

namespace SistemaAereo.Repositories.Interfaces
{
    public interface IFlightRepository : IRepository<Flight>
    {
        Task<IEnumerable<Flight>> GetFlightsCompleteAsync();
        Task<Flight> GetFlightCompleteAsync(int id);
        Task<Flight> GetFlightForEditAsync(int id);
        Task<IEnumerable<Flight>> GetUpcomingFlightsAsync(int quantity = 5);
        Task<IEnumerable<Flight>> GetFlightsByPeriodAsync(DateTime start, DateTime end);
        Task<IEnumerable<Flight>> GetFlightsByAirportAsync(int airportId);
        Task<IEnumerable<Flight>> GetAvailableFlightsAsync();
        Task<IEnumerable<Flight>> GetFlightsWithFiltersAsync(
            int? departureAirportId = null,
            int? arrivalAirportId = null,
            int? aircraftId = null,
            DateTime? startDate = null,
            DateTime? endDate = null,
            bool onlyWithAvailableSeats = false);
        Task<bool> FlightNumberExistsAsync(string flightNumber, int? excludeId = null);
        Task<bool> HasStopoversAsync(int flightId);
        Task<bool> HasSeatsAsync(int flightId);
        Task<bool> HasOccupiedSeatsAsync(int flightId);
        Task<int> GetTotalFlightsAsync();
        Task<int> GetTotalFlightsByAirportAsync(int airportId);
        Task<int> GetTotalFlightsByPeriodAsync(DateTime start, DateTime end);
        Task<int> GetTotalAvailableSeatsAsync(int flightId);
        Task<int> GetTotalOccupiedSeatsAsync(int flightId);
        Task<IEnumerable<Flight>> GetFlightsTodayAsync();
        Task<IEnumerable<Flight>> GetFlightsByStatusAsync(string status);
        Task<Dictionary<string, int>> GetFlightStatisticsByAirportAsync();
        Task UpdateFlightsStatusAsync();
        Task CancelFlightsWithLowOccupancyAsync(double minimumPercentage);
        Task<(IEnumerable<Flight> Flights, int TotalCount)> GetPaginatedFlightsAsync(
            int page = 1,
            int itemsPerPage = 10,
            string sortBy = "date",
            bool ascending = true);
        new Task<IEnumerable<Flight>> GetAllAsync();
        new Task<IEnumerable<Flight>> FindAsync(Expression<Func<Flight, bool>> predicate);
    }
}