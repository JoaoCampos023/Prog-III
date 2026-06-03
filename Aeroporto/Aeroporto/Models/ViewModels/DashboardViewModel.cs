using SistemaAereo.Models.Entities;

namespace SistemaAereo.Models.ViewModels
{
    public class DashboardViewModel
    {
        // =============================================
        // ESTATÍSTICAS PRINCIPAIS
        // =============================================

        public int TotalFlights { get; set; }
        public int TotalCustomers { get; set; }
        public int TotalAircrafts { get; set; }
        public int TotalAirports { get; set; }

        // =============================================
        // ESTATÍSTICAS DE PASSAGENS
        // =============================================

        public int TotalTickets { get; set; }
        public int ConfirmedTickets { get; set; }
        public int CheckInTickets { get; set; }
        public int BoardedTickets { get; set; }
        public int CancelledTickets { get; set; }

        // =============================================
        // DADOS FINANCEIROS
        // =============================================

        public decimal TotalRevenue { get; set; }
        public decimal CurrentMonthRevenue { get; set; }

        // =============================================
        // LISTAS DE DADOS
        // =============================================

        public List<Flight> UpcomingFlights { get; set; } = new List<Flight>();
        public List<Ticket> RecentTickets { get; set; } = new List<Ticket>();
    }
}