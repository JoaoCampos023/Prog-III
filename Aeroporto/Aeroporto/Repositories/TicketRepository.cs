using Microsoft.EntityFrameworkCore;
using SistemaAereo.Data.Context;
using SistemaAereo.Models.Entities;
using SistemaAereo.Models.Enums;
using SistemaAereo.Repositories.Interfaces;

namespace SistemaAereo.Repositories
{
    // Implementação do repositório de passagens
    public class TicketRepository : Repository<Ticket>, ITicketRepository
    {
        public TicketRepository(AirportsContext context) : base(context) { }

        // Obtém todas as passagens com dados relacionados (cliente, voo, poltrona)
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

        // Obtém uma passagem específica com todos os dados relacionados
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

        // Obtém todas as passagens de um cliente específico
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

        // Obtém todas as passagens de um voo específico
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

        // Verifica se um número de bilhete já existe
        public async Task<bool> TicketNumberExistsAsync(string ticketNumber)
        {
            return await _dbSet.AnyAsync(t => t.TicketNumber == ticketNumber);
        }

        // Verifica se uma poltrona está ocupada em um voo
        public async Task<bool> IsSeatOccupiedAsync(int flightId, int seatId)
        {
            // Uma poltrona está ocupada se existe uma passagem ativa (não cancelada) para ela
            return await _dbSet.AnyAsync(t =>
                t.FlightId == flightId &&
                t.SeatId == seatId &&
                t.Status != TicketStatus.Cancelled);
        }

        // Total de passagens vendidas para um voo
        public async Task<int> GetTotalTicketsSoldByFlightAsync(int flightId)
        {
            return await _dbSet.CountAsync(t =>
                t.FlightId == flightId &&
                t.Status != TicketStatus.Cancelled);
        }

        // Faturamento total de um voo
        public async Task<decimal> GetRevenueByFlightAsync(int flightId)
        {
            return await _dbSet
                .Where(t => t.FlightId == flightId && t.Status != TicketStatus.Cancelled)
                .SumAsync(t => t.Price);
        }
    }
}