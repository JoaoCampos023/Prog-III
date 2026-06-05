using SistemaAereo.Models.Entities;

namespace SistemaAereo.Repositories.Interfaces
{
    // Interface específica para o repositório de passagens
    // Herda os métodos genéricos do IRepository
    public interface ITicketRepository : IRepository<Ticket>
    {
        // =============================================
        // CONSULTAS COMPLEXAS COM INCLUDE
        // =============================================

        // Obtém todas as passagens com dados relacionados (cliente, voo, poltrona)
        Task<IEnumerable<Ticket>> GetTicketsCompleteAsync();

        // Obtém uma passagem específica com todos os dados relacionados
        Task<Ticket> GetTicketCompleteAsync(int id);

        // =============================================
        // CONSULTAS FILTRADAS
        // =============================================

        // Obtém todas as passagens de um cliente específico
        Task<IEnumerable<Ticket>> GetTicketsByCustomerAsync(int customerId);

        // Obtém todas as passagens de um voo específico
        Task<IEnumerable<Ticket>> GetTicketsByFlightAsync(int flightId);

        // =============================================
        // VALIDAÇÕES
        // =============================================

        // Verifica se um número de bilhete já existe
        Task<bool> TicketNumberExistsAsync(string ticketNumber);

        // Verifica se uma poltrona está ocupada em um voo
        Task<bool> IsSeatOccupiedAsync(int flightId, int seatId);

        // =============================================
        // ESTATÍSTICAS
        // =============================================

        // Total de passagens vendidas para um voo
        Task<int> GetTotalTicketsSoldByFlightAsync(int flightId);

        // Faturamento total de um voo
        Task<decimal> GetRevenueByFlightAsync(int flightId);
    }
}