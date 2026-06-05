using SistemaAereo.Models.Entities;
using System.Linq.Expressions;

namespace SistemaAereo.Repositories.Interfaces
{
    // Interface específica para o repositório de voos
    // Herda os métodos genéricos do IRepository
    public interface IFlightRepository : IRepository<Flight>
    {
        // =============================================
        // CONSULTAS COMPLEXAS COM INCLUDE
        // =============================================

        // Obtém todos os voos com todos os dados relacionados
        Task<IEnumerable<Flight>> GetFlightsCompleteAsync();

        // Obtém um voo específico com todos os dados relacionados
        Task<Flight> GetFlightCompleteAsync(int id);

        // Obtém um voo para edição (com tracking)
        Task<Flight> GetFlightForEditAsync(int id);

        // =============================================
        // CONSULTAS FILTRADAS
        // =============================================

        // Obtém os próximos X voos (padrão: 5)
        Task<IEnumerable<Flight>> GetUpcomingFlightsAsync(int quantity = 5);

        // Obtém voos em um período específico
        Task<IEnumerable<Flight>> GetFlightsByPeriodAsync(DateTime start, DateTime end);

        // Obtém voos que passam por um determinado aeroporto (origem ou destino)
        Task<IEnumerable<Flight>> GetFlightsByAirportAsync(int airportId);

        // Obtém voos disponíveis para venda (futuros e com poltronas)
        Task<IEnumerable<Flight>> GetAvailableFlightsAsync();

        // =============================================
        // CONSULTAS COM MÚLTIPLOS FILTROS
        // =============================================

        // Obtém voos aplicando diversos filtros combinados
        Task<IEnumerable<Flight>> GetFlightsWithFiltersAsync(
            int? departureAirportId = null,
            int? arrivalAirportId = null,
            int? aircraftId = null,
            DateTime? startDate = null,
            DateTime? endDate = null,
            bool onlyWithAvailableSeats = false);

        // =============================================
        // VALIDAÇÕES
        // =============================================

        // Verifica se um número de voo já existe (excluindo um voo opcional)
        Task<bool> FlightNumberExistsAsync(string flightNumber, int? excludeId = null);

        // Verifica se o voo possui escalas
        Task<bool> HasStopoversAsync(int flightId);

        // Verifica se o voo possui poltronas
        Task<bool> HasSeatsAsync(int flightId);

        // Verifica se o voo possui poltronas ocupadas
        Task<bool> HasOccupiedSeatsAsync(int flightId);

        // =============================================
        // ESTATÍSTICAS
        // =============================================

        // Total de voos cadastrados
        Task<int> GetTotalFlightsAsync();

        // Total de voos que passam por um aeroporto
        Task<int> GetTotalFlightsByAirportAsync(int airportId);

        // Total de voos em um período
        Task<int> GetTotalFlightsByPeriodAsync(DateTime start, DateTime end);

        // Total de poltronas disponíveis em um voo
        Task<int> GetTotalAvailableSeatsAsync(int flightId);

        // Total de poltronas ocupadas em um voo
        Task<int> GetTotalOccupiedSeatsAsync(int flightId);

        // =============================================
        // CONSULTAS ESPECIALIZADAS
        // =============================================

        // Obtém voos que acontecem hoje
        Task<IEnumerable<Flight>> GetFlightsTodayAsync();

        // Obtém voos filtrados por status (futuros, hoje, passados)
        Task<IEnumerable<Flight>> GetFlightsByStatusAsync(string status);

        // Obtém estatísticas de voos agrupadas por aeroporto
        Task<Dictionary<string, int>> GetFlightStatisticsByAirportAsync();

        // =============================================
        // OPERAÇÕES EM LOTE
        // =============================================

        // Atualiza o status de voos antigos (job futuro)
        Task UpdateFlightsStatusAsync();

        // Cancela voos com ocupação abaixo do percentual mínimo
        Task CancelFlightsWithLowOccupancyAsync(double minimumPercentage);

        // =============================================
        // PAGINAÇÃO
        // =============================================

        // Obtém voos paginados com ordenação dinâmica
        Task<(IEnumerable<Flight> Flights, int TotalCount)> GetPaginatedFlightsAsync(
            int page = 1,
            int itemsPerPage = 10,
            string sortBy = "date",
            bool ascending = true);

        // =============================================
        // OVERRIDES DOS MÉTODOS BASE (COM INCLUDE)
        // =============================================

        // Sobrescreve o GetAll para incluir os dados relacionados
        new Task<IEnumerable<Flight>> GetAllAsync();

        // Sobrescreve o Find para incluir os dados relacionados
        new Task<IEnumerable<Flight>> FindAsync(Expression<Func<Flight, bool>> predicate);
    }
}