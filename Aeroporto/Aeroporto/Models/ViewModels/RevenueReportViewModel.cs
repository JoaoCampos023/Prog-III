namespace SistemaAereo.Models.ViewModels
{
    // ViewModel para o relatório de faturamento
    public class RevenueReportViewModel
    {
        // Data de início do período analisado
        public DateTime StartDate { get; set; }

        // Data de fim do período analisado
        public DateTime EndDate { get; set; }

        // Faturamento total do período
        public decimal TotalRevenue { get; set; }

        // Total de passagens vendidas no período
        public int TotalTickets { get; set; }

        // Ticket médio do período (média de preço por passagem)
        public decimal AverageTicketPrice { get; set; }

        // Faturamento detalhado por dia
        public List<DailyRevenueDto> DailyRevenue { get; set; }

        // Faturamento detalhado por mês
        public List<MonthlyRevenueDto> MonthlyRevenue { get; set; }
    }

    // DTO para faturamento diário
    public class DailyRevenueDto
    {
        // Data do faturamento
        public DateTime Date { get; set; }

        // Valor faturado no dia
        public decimal Amount { get; set; }

        // Quantidade de passagens vendidas
        public int Quantity { get; set; }
    }

    // DTO para faturamento mensal
    public class MonthlyRevenueDto
    {
        // Mês/Ano no formato YYYY-MM
        public string Month { get; set; }

        // Valor faturado no mês
        public decimal Amount { get; set; }

        // Quantidade de passagens vendidas
        public int Quantity { get; set; }
    }
}