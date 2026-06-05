using Microsoft.EntityFrameworkCore;
using SistemaAereo.Data.Context;
using SistemaAereo.Models.Entities;
using SistemaAereo.Repositories.Interfaces;

namespace SistemaAereo.Repositories
{
    // Implementação do repositório de clientes
    public class CustomerRepository : Repository<Customer>, ICustomerRepository
    {
        public CustomerRepository(AirportsContext context) : base(context) { }

        // =============================================
        // CONSULTAS DE CLIENTES POR STATUS
        // =============================================

        // Obtém apenas os clientes ativos
        public async Task<IEnumerable<Customer>> GetActiveCustomersAsync()
        {
            return await _dbSet
                .Where(c => c.IsActive)
                .OrderBy(c => c.Name)
                .ToListAsync();
        }

        // Obtém apenas os clientes inativos
        public async Task<IEnumerable<Customer>> GetInactiveCustomersAsync()
        {
            return await _dbSet
                .Where(c => !c.IsActive)
                .OrderBy(c => c.Name)
                .ToListAsync();
        }

        // Obtém todos os clientes (ativos e inativos)
        public async Task<IEnumerable<Customer>> GetAllCustomersAsync()
        {
            return await _dbSet
                .OrderBy(c => c.Name)
                .ToListAsync();
        }

        // =============================================
        // CONTAGENS
        // =============================================

        // Total de clientes ativos
        public async Task<int> GetTotalActiveCustomersAsync()
        {
            return await _dbSet.CountAsync(c => c.IsActive);
        }

        // Total de clientes inativos
        public async Task<int> GetTotalInactiveCustomersAsync()
        {
            return await _dbSet.CountAsync(c => !c.IsActive);
        }

        // =============================================
        // VALIDAÇÕES DE UNICIDADE
        // =============================================

        // Verifica se um email já existe
        public async Task<bool> EmailExistsAsync(string email, int? excludeId = null)
        {
            // Se excludeId foi informado, ignora o cliente com esse ID
            if (excludeId.HasValue)
                return await _dbSet.AnyAsync(c =>
                    c.Email == email &&
                    c.CustomerId != excludeId.Value);

            return await _dbSet.AnyAsync(c => c.Email == email);
        }

        // Verifica se um CPF já existe
        public async Task<bool> CPFExistsAsync(string cpf, int? excludeId = null)
        {
            // CPF vazio não precisa ser verificado
            if (string.IsNullOrEmpty(cpf)) return false;

            // Se excludeId foi informado, ignora o cliente com esse ID
            if (excludeId.HasValue)
                return await _dbSet.AnyAsync(c =>
                    c.CPF == cpf &&
                    c.CustomerId != excludeId.Value);

            return await _dbSet.AnyAsync(c => c.CPF == cpf);
        }
    }
}