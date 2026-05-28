using Microsoft.EntityFrameworkCore;
using SistemaAereo.Data.Context;
using SistemaAereo.Models.Entities;
using SistemaAereo.Repositories.Interfaces;

namespace SistemaAereo.Repositories
{
    public class CustomerRepository : Repository<Customer>, ICustomerRepository
    {
        public CustomerRepository(AirportsContext context) : base(context) { }

        public async Task<IEnumerable<Customer>> GetActiveCustomersAsync()
        {
            return await _dbSet
                .Where(c => c.IsActive)
                .OrderBy(c => c.Name)
                .ToListAsync();
        }

        public async Task<IEnumerable<Customer>> GetInactiveCustomersAsync()
        {
            return await _dbSet
                .Where(c => !c.IsActive)
                .OrderBy(c => c.Name)
                .ToListAsync();
        }

        public async Task<IEnumerable<Customer>> GetAllCustomersAsync()
        {
            return await _dbSet
                .OrderBy(c => c.Name)
                .ToListAsync();
        }

        public async Task<int> GetTotalActiveCustomersAsync()
        {
            return await _dbSet.CountAsync(c => c.IsActive);
        }

        public async Task<int> GetTotalInactiveCustomersAsync()
        {
            return await _dbSet.CountAsync(c => !c.IsActive);
        }

        public async Task<bool> EmailExistsAsync(string email, int? excludeId = null)
        {
            if (excludeId.HasValue)
                return await _dbSet.AnyAsync(c =>
                    c.Email == email &&
                    c.CustomerId != excludeId.Value);

            return await _dbSet.AnyAsync(c => c.Email == email);
        }

        public async Task<bool> CPFExistsAsync(string cpf, int? excludeId = null)
        {
            if (string.IsNullOrEmpty(cpf)) return false;

            if (excludeId.HasValue)
                return await _dbSet.AnyAsync(c =>
                    c.CPF == cpf &&
                    c.CustomerId != excludeId.Value);

            return await _dbSet.AnyAsync(c => c.CPF == cpf);
        }
    }
}