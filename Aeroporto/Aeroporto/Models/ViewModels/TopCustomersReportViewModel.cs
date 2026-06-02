namespace SistemaAereo.Models.ViewModels
{
    /// <summary>
    /// ViewModel para o relatório de clientes mais frequentes
    /// </summary>
    public class TopCustomersReportViewModel
    {
        /// <summary>Lista dos clientes mais frequentes</summary>
        public List<TopCustomerDto> TopCustomers { get; set; }

        /// <summary>Total de clientes cadastrados</summary>
        public int TotalCustomers { get; set; }

        /// <summary>Total de clientes ativos</summary>
        public int TotalActiveCustomers { get; set; }
    }

    /// <summary>
    /// DTO para cliente frequente
    /// </summary>
    public class TopCustomerDto
    {
        /// <summary>Identificador do cliente</summary>
        public int CustomerId { get; set; }

        /// <summary>Nome do cliente</summary>
        public string Name { get; set; }

        /// <summary>Email do cliente</summary>
        public string Email { get; set; }

        /// <summary>Total de passagens compradas</summary>
        public int TotalTickets { get; set; }

        /// <summary>Valor total gasto</summary>
        public decimal TotalAmount { get; set; }

        /// <summary>Ticket médio do cliente</summary>
        public decimal AverageTicketPrice { get; set; }
    }
}