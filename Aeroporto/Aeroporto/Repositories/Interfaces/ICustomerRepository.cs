using SistemaAereo.Models.Entities;

namespace SistemaAereo.Repositories.Interfaces
{
    public interface ICustomerRepository : IRepository<Customer>
    {
        Task<IEnumerable<Customer>> GetActiveCustomersAsync();
        Task<IEnumerable<Customer>> GetInactiveCustomersAsync();
        Task<IEnumerable<Customer>> GetAllCustomersAsync();
        Task<int> GetTotalActiveCustomersAsync();
        Task<int> GetTotalInactiveCustomersAsync();
        Task<bool> EmailExistsAsync(string email, int? excludeId = null);
        Task<bool> CPFExistsAsync(string cpf, int? excludeId = null);
    }
}