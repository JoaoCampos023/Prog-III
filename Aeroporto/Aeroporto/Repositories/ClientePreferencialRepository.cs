using Microsoft.EntityFrameworkCore;
using SistemaAereo.Data;
using SistemaAereo.Models;
using SistemaAereo.Repositories.Interfaces;

namespace SistemaAereo.Repositories
{
    public class ClientePreferencialRepository : Repository<ClientePreferencial>, IClientePreferencialRepository
    {
        // =============================================
        // CONSTRUTOR
        // =============================================

        public ClientePreferencialRepository(AeroportoContext context) : base(context) { }

        // =============================================
        // IMPLEMENTAÇÃO - CONSULTAS DE CLIENTES
        // =============================================

        public async Task<IEnumerable<ClientePreferencial>> GetClientesAtivosAsync()
        {
            return await _dbSet
                .Where(c => c.Ativo)
                .OrderBy(c => c.Nome)
                .ToListAsync();
        }

        public async Task<IEnumerable<ClientePreferencial>> GetClientesInativosAsync()
        {
            return await _dbSet
                .Where(c => !c.Ativo)
                .OrderBy(c => c.Nome)
                .ToListAsync();
        }

        public async Task<IEnumerable<ClientePreferencial>> GetAllClientesAsync()
        {
            return await _dbSet
                .OrderBy(c => c.Nome)
                .ToListAsync();
        }

        public async Task<int> GetTotalClientesAtivosAsync()
        {
            return await _dbSet.CountAsync(c => c.Ativo);
        }

        public async Task<int> GetTotalClientesInativosAsync()
        {
            return await _dbSet.CountAsync(c => !c.Ativo);
        }

        // =============================================
        // IMPLEMENTAÇÃO - VALIDAÇÕES DE UNICIDADE
        // =============================================

        public async Task<bool> EmailExistsAsync(string email, int? excludeId = null)
        {
            if (excludeId.HasValue)
                return await _dbSet.AnyAsync(c =>
                    c.Email == email &&
                    c.ClienteId != excludeId.Value);

            return await _dbSet.AnyAsync(c => c.Email == email);
        }

        public async Task<bool> CPFExistsAsync(string cpf, int? excludeId = null)
        {
            if (string.IsNullOrEmpty(cpf)) return false;

            if (excludeId.HasValue)
                return await _dbSet.AnyAsync(c =>
                    c.CPF == cpf &&
                    c.ClienteId != excludeId.Value);

            return await _dbSet.AnyAsync(c => c.CPF == cpf);
        }
    }
}