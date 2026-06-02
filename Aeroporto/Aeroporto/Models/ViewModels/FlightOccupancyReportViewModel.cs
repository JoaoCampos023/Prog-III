namespace SistemaAereo.Models.ViewModels
{
    /// <summary>
    /// ViewModel para o relatório de ocupação de voos
    /// </summary>
    public class FlightOccupancyReportViewModel
    {
        /// <summary>Lista de voos com suas ocupações</summary>
        public List<FlightOccupancyDto> Flights { get; set; }

        /// <summary>Percentual médio de ocupação</summary>
        public double AverageOccupancy { get; set; }

        /// <summary>Total de voos no período</summary>
        public int TotalFlights { get; set; }

        /// <summary>Total de passageiros transportados</summary>
        public int TotalPassengers { get; set; }
    }

    /// <summary>
    /// DTO para ocupação de um voo específico
    /// </summary>
    public class FlightOccupancyDto
    {
        /// <summary>Identificador do voo</summary>
        public int FlightId { get; set; }

        /// <summary>Número do voo</summary>
        public string FlightNumber { get; set; }

        /// <summary>Código IATA do aeroporto de origem</summary>
        public string Origin { get; set; }

        /// <summary>Código IATA do aeroporto de destino</summary>
        public string Destination { get; set; }

        /// <summary>Data e hora de saída</summary>
        public DateTime DepartureTime { get; set; }

        /// <summary>Total de poltronas do voo</summary>
        public int TotalSeats { get; set; }

        /// <summary>Total de poltronas ocupadas</summary>
        public int OccupiedSeats { get; set; }

        /// <summary>Percentual de ocupação</summary>
        public double OccupancyPercentage { get; set; }
    }
}