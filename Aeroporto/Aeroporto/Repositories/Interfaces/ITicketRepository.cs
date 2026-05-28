using SistemaAereo.Models.Entities;

namespace SistemaAereo.Repositories.Interfaces
{
    public interface ITicketRepository : IRepository<Ticket>
    {
        Task<IEnumerable<Ticket>> GetTicketsCompleteAsync();
        Task<Ticket> GetTicketCompleteAsync(int id);
        Task<IEnumerable<Ticket>> GetTicketsByCustomerAsync(int customerId);
        Task<IEnumerable<Ticket>> GetTicketsByFlightAsync(int flightId);
        Task<bool> TicketNumberExistsAsync(string ticketNumber);
        Task<bool> IsSeatOccupiedAsync(int flightId, int seatId);
        Task<int> GetTotalTicketsSoldByFlightAsync(int flightId);
        Task<decimal> GetRevenueByFlightAsync(int flightId);
    }
}