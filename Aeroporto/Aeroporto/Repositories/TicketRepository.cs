using Microsoft.EntityFrameworkCore;
using SistemaAereo.Data.Context;
using SistemaAereo.Models.Entities;
using SistemaAereo.Models.Enums;
using SistemaAereo.Repositories.Interfaces;

namespace SistemaAereo.Repositories
{
    public class TicketRepository : Repository<Ticket>, ITicketRepository
    {
        public TicketRepository(AirportsContext context) : base(context) { }

        /// <summary>
        /// Obtém todas as passagens com os dados completos (voos, clientes, poltronas)
        /// </summary>
        public async Task<IEnumerable<Ticket>> GetTicketsCompleteAsync()
        {
            return await _dbSet
                .AsNoTracking()
                .Include(t => t.Flight)
                    .ThenInclude(f => f.DepartureAirport)
                .Include(t => t.Flight)
                    .ThenInclude(f => f.ArrivalAirport)
                .Include(t => t.Customer)
                .Include(t => t.Seat)
                .OrderByDescending(t => t.IssueDate)
                .ToListAsync();
        }

        /// <summary>
        /// Obtém uma passagem específica com todos os dados relacionados
        /// </summary>
        public async Task<Ticket> GetTicketCompleteAsync(int id)
        {
            return await _dbSet
                .Include(t => t.Flight)
                    .ThenInclude(f => f.DepartureAirport)
                .Include(t => t.Flight)
                    .ThenInclude(f => f.ArrivalAirport)
                .Include(t => t.Flight)
                    .ThenInclude(f => f.Aircraft)
                .Include(t => t.Customer)
                .Include(t => t.Seat)
                .FirstOrDefaultAsync(t => t.TicketId == id);
        }

        /// <summary>
        /// Obtém todas as passagens de um cliente específico
        /// </summary>
        public async Task<IEnumerable<Ticket>> GetTicketsByCustomerAsync(int customerId)
        {
            return await _dbSet
                .AsNoTracking()
                .Include(t => t.Flight)
                    .ThenInclude(f => f.DepartureAirport)
                .Include(t => t.Flight)
                    .ThenInclude(f => f.ArrivalAirport)
                .Include(t => t.Seat)
                .Where(t => t.CustomerId == customerId)
                .OrderByDescending(t => t.IssueDate)
                .ToListAsync();
        }

        /// <summary>
        /// Obtém todas as passagens de um voo específico
        /// </summary>
        public async Task<IEnumerable<Ticket>> GetTicketsByFlightAsync(int flightId)
        {
            return await _dbSet
                .AsNoTracking()
                .Include(t => t.Customer)
                .Include(t => t.Seat)
                .Where(t => t.FlightId == flightId)
                .OrderBy(t => t.Seat.SeatNumber)
                .ToListAsync();
        }

        /// <summary>
        /// Verifica se um número de bilhete já existe
        /// </summary>
        public async Task<bool> TicketNumberExistsAsync(string ticketNumber)
        {
            return await _dbSet.AnyAsync(t => t.TicketNumber == ticketNumber);
        }

        /// <summary>
        /// Verifica se uma poltrona está ocupada em um voo
        /// </summary>
        public async Task<bool> IsSeatOccupiedAsync(int flightId, int seatId)
        {
            return await _dbSet.AnyAsync(t =>
                t.FlightId == flightId &&
                t.SeatId == seatId &&
                t.Status != TicketStatus.Cancelled);
        }

        /// <summary>
        /// Obtém o total de passagens vendidas para um voo
        /// </summary>
        public async Task<int> GetTotalTicketsSoldByFlightAsync(int flightId)
        {
            return await _dbSet.CountAsync(t =>
                t.FlightId == flightId &&
                t.Status != TicketStatus.Cancelled);
        }

        /// <summary>
        /// Obtém o faturamento total de um voo
        /// </summary>
        public async Task<decimal> GetRevenueByFlightAsync(int flightId)
        {
            return await _dbSet
                .Where(t => t.FlightId == flightId && t.Status != TicketStatus.Cancelled)
                .SumAsync(t => t.Price);
        }
    }
}