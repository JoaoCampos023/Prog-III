using SistemaAereo.Models.Entities;

namespace SistemaAereo.Services.Interfaces
{
    public interface ISeatService
    {

        /// Cria as poltronas para um voo específico
        /// 
        Task<List<Seat>> CreateSeatsForFlightAsync(int flightId, int? numberOfSeats = null);


        /// Verifica se o voo já possui poltronas

        Task<bool> HasSeatsAsync(int flightId);

        /// Obtém o total de poltronas disponíveis em um voo
        /// 
        Task<int> GetTotalAvailableSeatsAsync(int flightId);

        /// Obtém o total de poltronas ocupadas em um voo
        /// 
        Task<int> GetTotalOccupiedSeatsAsync(int flightId);
    }
}