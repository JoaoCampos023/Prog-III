using SistemaAereo.Models.Entities;

namespace SistemaAereo.Models.ViewModels
{
    /// <summary>
    /// ViewModel para o relatório de faturamento
    /// </summary>
    public class RevenueReportViewModel
    {
        /// <summary>Data de início do período</summary>
        public DateTime StartDate { get; set; }

        /// <summary>Data de fim do período</summary>
        public DateTime EndDate { get; set; }

        /// <summary>Faturamento total do período</summary>
        public decimal TotalRevenue { get; set; }

        /// <summary>Total de passagens vendidas</summary>
        public int TotalTickets { get; set; }

        /// <summary>Ticket médio do período</summary>
        public decimal AverageTicketPrice { get; set; }

        /// <summary>Faturamento detalhado por dia</summary>
        public List<DailyRevenueDto> DailyRevenue { get; set; }

        /// <summary>Faturamento detalhado por mês</summary>
        public List<MonthlyRevenueDto> MonthlyRevenue { get; set; }
    }

    /// <summary>
    /// DTO para faturamento diário
    /// </summary>
    public class DailyRevenueDto
    {
        /// <summary>Data do faturamento</summary>
        public DateTime Date { get; set; }

        /// <summary>Valor faturado no dia</summary>
        public decimal Amount { get; set; }

        /// <summary>Quantidade de passagens vendidas</summary>
        public int Quantity { get; set; }
    }

    /// <summary>
    /// DTO para faturamento mensal
    /// </summary>
    public class MonthlyRevenueDto
    {
        /// <summary>Mês/Ano no formato YYYY-MM</summary>
        public string Month { get; set; }

        /// <summary>Valor faturado no mês</summary>
        public decimal Amount { get; set; }

        /// <summary>Quantidade de passagens vendidas</summary>
        public int Quantity { get; set; }
    }
}