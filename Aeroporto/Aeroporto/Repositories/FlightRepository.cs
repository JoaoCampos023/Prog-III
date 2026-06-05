using Microsoft.EntityFrameworkCore;
using SistemaAereo.Data.Context;
using SistemaAereo.Models.Entities;
using SistemaAereo.Models.Enums;
using SistemaAereo.Repositories.Interfaces;
using System.Linq.Expressions;

namespace SistemaAereo.Repositories
{
    // Implementação do repositório de voos
    public class FlightRepository : Repository<Flight>, IFlightRepository
    {
        private readonly AirportsContext _context;

        public FlightRepository(AirportsContext context) : base(context)
        {
            _context = context;
        }

        // =============================================
        // CONSULTAS COMPLEXAS COM INCLUDE
        // =============================================

        // Obtém todos os voos com todos os dados relacionados
        public async Task<IEnumerable<Flight>> GetFlightsCompleteAsync()
        {
            return await _dbSet
                .AsNoTracking()
                .Include(f => f.DepartureAirport)
                .Include(f => f.ArrivalAirport)
                .Include(f => f.Aircraft)
                .Include(f => f.Stopovers)
                    .ThenInclude(s => s.Airport)
                .Include(f => f.Seats)
                .OrderBy(f => f.DepartureTime)
                .ToListAsync();
        }

        // Obtém um voo específico com todos os dados relacionados
        public async Task<Flight> GetFlightCompleteAsync(int id)
        {
            return await _dbSet
                .AsNoTracking()
                .Include(f => f.DepartureAirport)
                .Include(f => f.ArrivalAirport)
                .Include(f => f.Aircraft)
                .Include(f => f.Stopovers)
                    .ThenInclude(s => s.Airport)
                .Include(f => f.Seats)
                .FirstOrDefaultAsync(f => f.FlightId == id);
        }

        // Obtém um voo para edição (com tracking)
        public async Task<Flight> GetFlightForEditAsync(int id)
        {
            return await _dbSet
                .Include(f => f.DepartureAirport)
                .Include(f => f.ArrivalAirport)
                .Include(f => f.Aircraft)
                .Include(f => f.Stopovers)
                .Include(f => f.Seats)
                .FirstOrDefaultAsync(f => f.FlightId == id);
        }

        // =============================================
        // CONSULTAS FILTRADAS
        // =============================================

        // Obtém os próximos X voos
        public async Task<IEnumerable<Flight>> GetUpcomingFlightsAsync(int quantity = 5)
        {
            return await _dbSet
                .AsNoTracking()
                .Include(f => f.DepartureAirport)
                .Include(f => f.ArrivalAirport)
                .Include(f => f.Aircraft)
                .Where(f => f.DepartureTime > DateTime.Now)
                .OrderBy(f => f.DepartureTime)
                .Take(quantity)
                .ToListAsync();
        }

        // Obtém voos em um período específico
        public async Task<IEnumerable<Flight>> GetFlightsByPeriodAsync(DateTime start, DateTime end)
        {
            return await _dbSet
                .AsNoTracking()
                .Include(f => f.DepartureAirport)
                .Include(f => f.ArrivalAirport)
                .Include(f => f.Aircraft)
                .Where(f => f.DepartureTime >= start && f.DepartureTime <= end)
                .OrderBy(f => f.DepartureTime)
                .ToListAsync();
        }

        // Obtém voos que passam por um determinado aeroporto
        public async Task<IEnumerable<Flight>> GetFlightsByAirportAsync(int airportId)
        {
            return await _dbSet
                .AsNoTracking()
                .Include(f => f.DepartureAirport)
                .Include(f => f.ArrivalAirport)
                .Include(f => f.Aircraft)
                .Where(f => f.DepartureAirportId == airportId || f.ArrivalAirportId == airportId)
                .OrderBy(f => f.DepartureTime)
                .ToListAsync();
        }

        // Obtém voos disponíveis para venda
        public async Task<IEnumerable<Flight>> GetAvailableFlightsAsync()
        {
            return await _dbSet
                .AsNoTracking()
                .Include(f => f.DepartureAirport)
                .Include(f => f.ArrivalAirport)
                .Include(f => f.Aircraft)
                .Include(f => f.Seats)
                .Where(f => f.DepartureTime > DateTime.Now && f.Seats.Any(s => s.IsAvailable))
                .OrderBy(f => f.DepartureTime)
                .ToListAsync();
        }

        // =============================================
        // CONSULTAS COM MÚLTIPLOS FILTROS
        // =============================================

        // Obtém voos aplicando diversos filtros combinados
        public async Task<IEnumerable<Flight>> GetFlightsWithFiltersAsync(
            int? departureAirportId = null,
            int? arrivalAirportId = null,
            int? aircraftId = null,
            DateTime? startDate = null,
            DateTime? endDate = null,
            bool onlyWithAvailableSeats = false)
        {
            var query = _dbSet
                .AsNoTracking()
                .Include(f => f.DepartureAirport)
                .Include(f => f.ArrivalAirport)
                .Include(f => f.Aircraft)
                .AsQueryable();

            if (departureAirportId.HasValue)
                query = query.Where(f => f.DepartureAirportId == departureAirportId.Value);

            if (arrivalAirportId.HasValue)
                query = query.Where(f => f.ArrivalAirportId == arrivalAirportId.Value);

            if (aircraftId.HasValue)
                query = query.Where(f => f.AircraftId == aircraftId.Value);

            if (startDate.HasValue)
                query = query.Where(f => f.DepartureTime >= startDate.Value);

            if (endDate.HasValue)
                query = query.Where(f => f.DepartureTime <= endDate.Value);

            if (onlyWithAvailableSeats)
            {
                query = query.Include(f => f.Seats)
                            .Where(f => f.Seats.Any(s => s.IsAvailable));
            }

            return await query
                .OrderBy(f => f.DepartureTime)
                .ToListAsync();
        }

        // =============================================
        // VALIDAÇÕES
        // =============================================

        // Verifica se um número de voo já existe
        public async Task<bool> FlightNumberExistsAsync(string flightNumber, int? excludeId = null)
        {
            var query = _dbSet.AsNoTracking().Where(f => f.FlightNumber == flightNumber);

            if (excludeId.HasValue)
                query = query.Where(f => f.FlightId != excludeId.Value);

            return await query.AnyAsync();
        }

        // Verifica se o voo possui escalas
        public async Task<bool> HasStopoversAsync(int flightId)
        {
            return await _context.Stopovers.AnyAsync(s => s.FlightId == flightId);
        }

        // Verifica se o voo possui poltronas
        public async Task<bool> HasSeatsAsync(int flightId)
        {
            return await _context.Seats.AnyAsync(s => s.FlightId == flightId);
        }

        // Verifica se o voo possui poltronas ocupadas
        public async Task<bool> HasOccupiedSeatsAsync(int flightId)
        {
            return await _context.Seats.AnyAsync(s => s.FlightId == flightId && !s.IsAvailable);
        }

        // =============================================
        // ESTATÍSTICAS
        // =============================================

        // Total de voos cadastrados
        public async Task<int> GetTotalFlightsAsync()
        {
            return await _dbSet.AsNoTracking().CountAsync();
        }

        // Total de voos que passam por um aeroporto
        public async Task<int> GetTotalFlightsByAirportAsync(int airportId)
        {
            return await _dbSet
                .AsNoTracking()
                .CountAsync(f => f.DepartureAirportId == airportId || f.ArrivalAirportId == airportId);
        }

        // Total de voos em um período
        public async Task<int> GetTotalFlightsByPeriodAsync(DateTime start, DateTime end)
        {
            return await _dbSet
                .AsNoTracking()
                .CountAsync(f => f.DepartureTime >= start && f.DepartureTime <= end);
        }

        // Total de poltronas disponíveis em um voo
        public async Task<int> GetTotalAvailableSeatsAsync(int flightId)
        {
            return await _context.Seats
                .AsNoTracking()
                .CountAsync(s => s.FlightId == flightId && s.IsAvailable);
        }

        // Total de poltronas ocupadas em um voo
        public async Task<int> GetTotalOccupiedSeatsAsync(int flightId)
        {
            return await _context.Seats
                .AsNoTracking()
                .CountAsync(s => s.FlightId == flightId && !s.IsAvailable);
        }

        // =============================================
        // CONSULTAS ESPECIALIZADAS
        // =============================================

        // Obtém voos que acontecem hoje
        public async Task<IEnumerable<Flight>> GetFlightsTodayAsync()
        {
            var today = DateTime.Today;
            var tomorrow = today.AddDays(1);

            return await _dbSet
                .AsNoTracking()
                .Include(f => f.DepartureAirport)
                .Include(f => f.ArrivalAirport)
                .Include(f => f.Aircraft)
                .Where(f => f.DepartureTime >= today && f.DepartureTime < tomorrow)
                .OrderBy(f => f.DepartureTime)
                .ToListAsync();
        }

        // Obtém voos filtrados por status (futuros, hoje, passados)
        public async Task<IEnumerable<Flight>> GetFlightsByStatusAsync(string status)
        {
            var now = DateTime.Now;
            var query = _dbSet
                .AsNoTracking()
                .Include(f => f.DepartureAirport)
                .Include(f => f.ArrivalAirport)
                .Include(f => f.Aircraft)
                .AsQueryable();

            return status?.ToLower() switch
            {
                "upcoming" or "futuros" => await query.Where(f => f.DepartureTime > now)
                                          .OrderBy(f => f.DepartureTime).ToListAsync(),
                "past" or "passados" => await query.Where(f => f.DepartureTime <= now)
                                         .OrderByDescending(f => f.DepartureTime).ToListAsync(),
                "today" or "hoje" => await GetFlightsTodayAsync(),
                _ => await query.OrderBy(f => f.DepartureTime).ToListAsync()
            };
        }

        // Obtém estatísticas de voos agrupadas por aeroporto
        public async Task<Dictionary<string, int>> GetFlightStatisticsByAirportAsync()
        {
            var airports = await _context.Airports.ToListAsync();
            var statistics = new Dictionary<string, int>();

            foreach (var airport in airports)
            {
                var total = await GetTotalFlightsByAirportAsync(airport.AirportId);
                statistics[airport.Name] = total;
            }

            return statistics;
        }

        // =============================================
        // OPERAÇÕES EM LOTE
        // =============================================

        // Atualiza o status de voos antigos (job futuro)
        public async Task UpdateFlightsStatusAsync()
        {
            var oldFlights = await _dbSet
                .Where(f => f.DepartureTime < DateTime.Now.AddMonths(-6))
                .ToListAsync();

            // Lógica de atualização em lote pode ser implementada aqui
            foreach (var flight in oldFlights)
            {
                // Operações em lote (ex: marcar como arquivado)
            }

            await _context.SaveChangesAsync();
        }

        // Cancela voos com ocupação abaixo do percentual mínimo
        public async Task CancelFlightsWithLowOccupancyAsync(double minimumPercentage)
        {
            var upcomingFlights = await _dbSet
                .Include(f => f.Seats)
                .Where(f => f.DepartureTime > DateTime.Now)
                .ToListAsync();

            foreach (var flight in upcomingFlights)
            {
                var totalSeats = flight.Seats.Count;
                var occupiedSeats = flight.Seats.Count(s => !s.IsAvailable);
                var occupancy = totalSeats > 0 ? (double)occupiedSeats / totalSeats : 0;

                if (occupancy < minimumPercentage)
                {
                    // Lógica para cancelar voo (ex: mudar status, notificar clientes)
                }
            }

            await _context.SaveChangesAsync();
        }

        // =============================================
        // PAGINAÇÃO
        // =============================================

        // Obtém voos paginados com ordenação dinâmica
        public async Task<(IEnumerable<Flight> Flights, int TotalCount)> GetPaginatedFlightsAsync(
            int page = 1,
            int itemsPerPage = 10,
            string sortBy = "date",
            bool ascending = true)
        {
            var query = _dbSet
                .AsNoTracking()
                .Include(f => f.DepartureAirport)
                .Include(f => f.ArrivalAirport)
                .Include(f => f.Aircraft)
                .AsQueryable();

            // Aplica ordenação baseada no parâmetro
            query = sortBy?.ToLower() switch
            {
                "number" => ascending ? query.OrderBy(f => f.FlightNumber) : query.OrderByDescending(f => f.FlightNumber),
                "departure" => ascending ? query.OrderBy(f => f.DepartureAirport.Name) : query.OrderByDescending(f => f.DepartureAirport.Name),
                "arrival" => ascending ? query.OrderBy(f => f.ArrivalAirport.Name) : query.OrderByDescending(f => f.ArrivalAirport.Name),
                _ => ascending ? query.OrderBy(f => f.DepartureTime) : query.OrderByDescending(f => f.DepartureTime)
            };

            var totalCount = await query.CountAsync();
            var flights = await query
                .Skip((page - 1) * itemsPerPage)
                .Take(itemsPerPage)
                .ToListAsync();

            return (flights, totalCount);
        }

        // =============================================
        // OVERRIDES DOS MÉTODOS BASE
        // =============================================

        // Sobrescreve o GetAll para incluir os dados relacionados
        public override async Task<IEnumerable<Flight>> GetAllAsync()
        {
            return await _dbSet
                .AsNoTracking()
                .Include(f => f.DepartureAirport)
                .Include(f => f.ArrivalAirport)
                .Include(f => f.Aircraft)
                .OrderBy(f => f.DepartureTime)
                .ToListAsync();
        }

        // Sobrescreve o Find para incluir os dados relacionados
        public override async Task<IEnumerable<Flight>> FindAsync(Expression<Func<Flight, bool>> predicate)
        {
            return await _dbSet
                .AsNoTracking()
                .Include(f => f.DepartureAirport)
                .Include(f => f.ArrivalAirport)
                .Include(f => f.Aircraft)
                .Where(predicate)
                .OrderBy(f => f.DepartureTime)
                .ToListAsync();
        }
    }
}