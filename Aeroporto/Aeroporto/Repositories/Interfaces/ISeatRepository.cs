using SistemaAereo.Models.Entities;

namespace SistemaAereo.Repositories.Interfaces
{
    // Interface específica para o repositório de poltronas
    // Herda os métodos genéricos do IRepository
    public interface ISeatRepository : IRepository<Seat>
    {
        // =============================================
        // CONSULTAS DE POLTRONAS POR VOO
        // =============================================

        // Obtém todas as poltronas de um voo
        Task<IEnumerable<Seat>> GetSeatsByFlightAsync(int flightId);

        // Obtém apenas as poltronas disponíveis de um voo
        Task<IEnumerable<Seat>> GetAvailableSeatsByFlightAsync(int flightId);

        // Obtém uma poltrona com os dados do voo associado
        Task<Seat> GetSeatWithFlightAsync(int id);

        // =============================================
        // VALIDAÇÕES
        // =============================================

        // Verifica se um número de poltrona já existe em um voo
        Task<bool> SeatNumberExistsInFlightAsync(int flightId, string seatNumber);

        // =============================================
        // CONTAGENS
        // =============================================

        // Total de poltronas disponíveis em um voo
        Task<int> GetTotalAvailableSeatsByFlightAsync(int flightId);

        // Total de poltronas de um voo (disponíveis + ocupadas)
        Task<int> GetTotalSeatsByFlightAsync(int flightId);
    }
}