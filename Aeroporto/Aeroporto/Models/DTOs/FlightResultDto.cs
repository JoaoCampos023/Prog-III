using SistemaAereo.Models.Entities;

namespace SistemaAereo.Models.DTOs
{
    public class FlightResultDto
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public string ErrorMessage { get; set; }
        public Flight Flight { get; set; }
        public int FlightId { get; set; }

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

    public class FlightStatisticsDto
    {
        public int TotalSeats { get; set; }
        public int AvailableSeats { get; set; }
        public int OccupiedSeats { get; set; }
        public int TotalTickets { get; set; }
        public decimal TotalRevenue { get; set; }
        public double OccupancyPercentage { get; set; }
    }
}