using SistemaAereo.Models.Entities;

namespace SistemaAereo.Models.ViewModels
{
    /// <summary>
    /// ViewModel para o dashboard de clientes
    /// </summary>
    public class CustomerDashboardViewModel
    {
        /// <summary>Lista de clientes</summary>
        public IEnumerable<Customer> Customers { get; set; }

        /// <summary>Total de clientes</summary>
        public int TotalCustomers { get; set; }

        /// <summary>Total de clientes ativos</summary>
        public int ActiveCustomers { get; set; }

        /// <summary>Total de clientes inativos</summary>
        public int InactiveCustomers { get; set; }
    }
}