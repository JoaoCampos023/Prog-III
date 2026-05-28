using SistemaAereo.Models.DTOs;
using SistemaAereo.Models.Entities;

namespace SistemaAereo.Facades.Interfaces
{
    /// <summary>
    /// Fachada para operações complexas relacionadas a voos
    /// </summary>
    public interface IFlightFacade
    {
        /// <summary>
        /// Cria um novo voo com todas as dependências
        /// </summary>
        Task<FlightResultDto> CreateFlightAsync(Flight flight);

        /// <summary>
        /// Atualiza um voo existente
        /// </summary>
        Task<FlightResultDto> UpdateFlightAsync(Flight flight);

        /// <summary>
        /// Exclui um voo e todas suas dependências
        /// </summary>
        Task<FlightResultDto> DeleteFlightAsync(int flightId);

        /// <summary>
        /// Recria as poltronas de um voo
        /// </summary>
        Task<FlightResultDto> RecreateSeatsAsync(int flightId);

        /// <summary>
        /// Obtém estatísticas completas de um voo
        /// </summary>
        Task<FlightStatisticsDto> GetFlightStatisticsAsync(int flightId);
    }
}