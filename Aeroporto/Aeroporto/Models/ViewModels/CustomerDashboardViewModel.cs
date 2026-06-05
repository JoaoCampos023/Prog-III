using SistemaAereo.Models.Entities;

namespace SistemaAereo.Models.ViewModels
{
    // ViewModel para o dashboard de clientes
    public class CustomerDashboardViewModel
    {
        // Lista de clientes ativos
        public IEnumerable<Customer> Customers { get; set; }

        // Total de clientes cadastrados
        public int TotalCustomers { get; set; }

        // Total de clientes ativos
        public int ActiveCustomers { get; set; }

        // Total de clientes inativos
        public int InactiveCustomers { get; set; }
    }
}