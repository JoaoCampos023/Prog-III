using SistemaAereo.Models.DTOs;
using SistemaAereo.Models.Entities;

namespace SistemaAereo.Facades.Interfaces
{
    // Fachada para operações complexas relacionadas a voos
    // Simplifica as operações de criação, atualização, exclusão e estatísticas
    public interface IFlightFacade
    {
        // Cria um novo voo com todas as dependências
        // Inclui a criação automática das poltronas
        Task<FlightResultDto> CreateFlightAsync(Flight flight);

        // Atualiza um voo existente
        Task<FlightResultDto> UpdateFlightAsync(Flight flight);

        // Exclui um voo e todas suas dependências
        // Remove poltronas, escalas e verifica se há passagens
        Task<FlightResultDto> DeleteFlightAsync(int flightId);

        // Recria as poltronas de um voo
        // Útil para corrigir problemas na criação inicial
        Task<FlightResultDto> RecreateSeatsAsync(int flightId);

        // Obtém estatísticas completas de um voo
        // Retorna total de poltronas, ocupação, faturamento, etc.
        Task<FlightStatisticsDto> GetFlightStatisticsAsync(int flightId);
    }
}