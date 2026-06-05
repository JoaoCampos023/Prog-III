namespace SistemaAereo.Models.ViewModels
{
    // ViewModel para o relatório de ocupação de voos
    public class FlightOccupancyReportViewModel
    {
        // Lista de voos com suas ocupações
        public List<FlightOccupancyDto> Flights { get; set; }

        // Percentual médio de ocupação de todos os voos
        public double AverageOccupancy { get; set; }

        // Total de voos no período
        public int TotalFlights { get; set; }

        // Total de passageiros transportados
        public int TotalPassengers { get; set; }
    }

    // DTO para ocupação de um voo específico
    public class FlightOccupancyDto
    {
        // Identificador do voo
        public int FlightId { get; set; }

        // Número do voo
        public string FlightNumber { get; set; }

        // Código IATA do aeroporto de origem
        public string Origin { get; set; }

        // Código IATA do aeroporto de destino
        public string Destination { get; set; }

        // Data e hora de saída
        public DateTime DepartureTime { get; set; }

        // Total de poltronas do voo
        public int TotalSeats { get; set; }

        // Total de poltronas ocupadas
        public int OccupiedSeats { get; set; }

        // Percentual de ocupação (calculado)
        public double OccupancyPercentage { get; set; }
    }
}