using SistemaAereo.Models.Entities;

namespace SistemaAereo.Models.ViewModels
{
    // ViewModel para o dashboard principal
    public class DashboardViewModel
    {
        // =============================================
        // ESTATÍSTICAS PRINCIPAIS
        // =============================================

        // Total de voos ativos
        public int TotalFlights { get; set; }

        // Total de clientes ativos
        public int TotalCustomers { get; set; }

        // Total de aeronaves cadastradas
        public int TotalAircrafts { get; set; }

        // Total de aeroportos cadastrados
        public int TotalAirports { get; set; }

        // =============================================
        // ESTATÍSTICAS DE PASSAGENS
        // =============================================

        // Total de passagens emitidas
        public int TotalTickets { get; set; }

        // Passagens com status Confirmada
        public int ConfirmedTickets { get; set; }

        // Passagens com status Check-in
        public int CheckInTickets { get; set; }

        // Passagens com status Embarcada
        public int BoardedTickets { get; set; }

        // Passagens com status Cancelada
        public int CancelledTickets { get; set; }

        // =============================================
        // DADOS FINANCEIROS
        // =============================================

        // Faturamento total (exclui canceladas)
        public decimal TotalRevenue { get; set; }

        // Faturamento do mês atual
        public decimal CurrentMonthRevenue { get; set; }

        // =============================================
        // LISTAS DE DADOS
        // =============================================

        // Próximos voos programados
        public List<Flight> UpcomingFlights { get; set; } = new List<Flight>();

        // Passagens emitidas mais recentemente
        public List<Ticket> RecentTickets { get; set; } = new List<Ticket>();
    }
}