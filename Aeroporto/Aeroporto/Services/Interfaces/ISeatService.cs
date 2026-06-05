using SistemaAereo.Models.Entities;

namespace SistemaAereo.Services.Interfaces
{
    // Interface para o serviço de gerenciamento de poltronas
    public interface ISeatService
    {
        // Cria as poltronas para um voo específico
        // Gera automaticamente todas as poltronas baseado na capacidade da aeronave
        // Retorna a lista de poltronas criadas
        Task<List<Seat>> CreateSeatsForFlightAsync(int flightId, int? numberOfSeats = null);

        // Verifica se o voo já possui poltronas cadastradas
        Task<bool> HasSeatsAsync(int flightId);

        // Obtém o total de poltronas disponíveis em um voo
        Task<int> GetTotalAvailableSeatsAsync(int flightId);

        // Obtém o total de poltronas ocupadas em um voo
        Task<int> GetTotalOccupiedSeatsAsync(int flightId);
    }
}