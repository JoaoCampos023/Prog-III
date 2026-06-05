using SistemaAereo.Models.Entities;

namespace SistemaAereo.Repositories.Interfaces
{
    // Interface específica para o repositório de clientes
    // Herda os métodos genéricos do IRepository
    public interface ICustomerRepository : IRepository<Customer>
    {
        // =============================================
        // CONSULTAS DE CLIENTES POR STATUS
        // =============================================

        // Obtém apenas os clientes ativos
        Task<IEnumerable<Customer>> GetActiveCustomersAsync();

        // Obtém apenas os clientes inativos
        Task<IEnumerable<Customer>> GetInactiveCustomersAsync();

        // Obtém todos os clientes (ativos e inativos)
        Task<IEnumerable<Customer>> GetAllCustomersAsync();

        // =============================================
        // CONTAGENS
        // =============================================

        // Total de clientes ativos
        Task<int> GetTotalActiveCustomersAsync();

        // Total de clientes inativos
        Task<int> GetTotalInactiveCustomersAsync();

        // =============================================
        // VALIDAÇÕES DE UNICIDADE
        // =============================================

        // Verifica se um email já existe (excluindo um cliente opcional)
        Task<bool> EmailExistsAsync(string email, int? excludeId = null);

        // Verifica se um CPF já existe (excluindo um cliente opcional)
        Task<bool> CPFExistsAsync(string cpf, int? excludeId = null);
    }
}