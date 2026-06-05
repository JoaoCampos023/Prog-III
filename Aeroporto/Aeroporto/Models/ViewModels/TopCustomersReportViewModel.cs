namespace SistemaAereo.Models.ViewModels
{
    // ViewModel para o relatório de clientes mais frequentes
    public class TopCustomersReportViewModel
    {
        // Lista dos clientes mais frequentes (ranking)
        public List<TopCustomerDto> TopCustomers { get; set; }

        // Total de clientes cadastrados
        public int TotalCustomers { get; set; }

        // Total de clientes ativos
        public int TotalActiveCustomers { get; set; }
    }

    // DTO para cliente frequente (ranking)
    public class TopCustomerDto
    {
        // Identificador do cliente
        public int CustomerId { get; set; }

        // Nome do cliente
        public string Name { get; set; }

        // Email do cliente
        public string Email { get; set; }

        // Total de passagens compradas
        public int TotalTickets { get; set; }

        // Valor total gasto pelo cliente
        public decimal TotalAmount { get; set; }

        // Ticket médio do cliente (média por passagem)
        public decimal AverageTicketPrice { get; set; }
    }
}