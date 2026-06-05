using SistemaAereo.Models.Entities;

namespace SistemaAereo.Models.DTOs
{
    // DTO para resultado de operações com voos
    public class FlightResultDto
    {
        // Indica se a operação foi bem sucedida
        public bool Success { get; set; }

        // Mensagem amigável sobre o resultado
        public string Message { get; set; }

        // Mensagem de erro (quando Success = false)
        public string ErrorMessage { get; set; }

        // Objeto Flight completo (quando Success = true)
        public Flight Flight { get; set; }

        // ID do voo afetado
        public int FlightId { get; set; }

        // Cria um resultado de sucesso
        public static FlightResultDto Ok(Flight flight, string message = "Operação realizada com sucesso")
        {
            return new FlightResultDto
            {
                Success = true,
                Message = message,
                Flight = flight,
                FlightId = flight.FlightId
            };
        }

        // Cria um resultado de falha
        public static FlightResultDto Fail(string errorMessage)
        {
            return new FlightResultDto
            {
                Success = false,
                ErrorMessage = errorMessage,
                Message = "Falha na operação"
            };
        }
    }

    // DTO para estatísticas de um voo
    public class FlightStatisticsDto
    {
        // Total de poltronas do voo
        public int TotalSeats { get; set; }

        // Total de poltronas disponíveis
        public int AvailableSeats { get; set; }

        // Total de poltronas ocupadas
        public int OccupiedSeats { get; set; }

        // Total de passagens vendidas
        public int TotalTickets { get; set; }

        // Faturamento total do voo
        public decimal TotalRevenue { get; set; }

        // Percentual de ocupação (calculado)
        public double OccupancyPercentage { get; set; }
    }
}