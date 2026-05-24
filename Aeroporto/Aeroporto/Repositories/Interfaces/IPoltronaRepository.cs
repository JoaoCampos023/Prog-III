using Microsoft.EntityFrameworkCore;
using SistemaAereo.Data.Context;
using SistemaAereo.Models.Entities;
using SistemaAereo.Repositories.Interfaces;

namespace SistemaAereo.Repositories
{
    public interface IPoltronaRepository : IRepository<Poltrona>
    {
        // =============================================
        // CONSULTAS DE POLTRONAS
        // =============================================

        Task<IEnumerable<Poltrona>> GetPoltronasPorVooAsync(int vooId);
        Task<IEnumerable<Poltrona>> GetPoltronasDisponiveisPorVooAsync(int vooId);
        Task<Poltrona> GetPoltronaComVooAsync(int id);

        // =============================================
        // VALIDAÇÕES E VERIFICAÇÕES
        // =============================================

        Task<bool> NumeroPoltronaExistsInVooAsync(int vooId, string numeroPoltrona);

        // =============================================
        // ESTATÍSTICAS E CONTAGENS
        // =============================================

        Task<int> GetTotalPoltronasDisponiveisPorVooAsync(int vooId);
        Task<int> GetTotalPoltronasPorVooAsync(int vooId);
    }

    // =============================================
    // IMPLEMENTAÇÃO DO REPOSITÓRIO
    // =============================================

    public class PoltronaRepository : Repository<Poltrona>, IPoltronaRepository
    {
        public PoltronaRepository(AeroportoContext context) : base(context) { }

        // =============================================
        // IMPLEMENTAÇÃO - CONSULTAS DE POLTRONAS
        // =============================================

        public async Task<IEnumerable<Poltrona>> GetPoltronasPorVooAsync(int vooId)
        {
            return await _dbSet
                .AsNoTracking()
                .Include(p => p.Voo)
                .Where(p => p.VooId == vooId)
                .OrderBy(p => p.NumeroPoltrona)
                .ToListAsync();
        }

        public async Task<IEnumerable<Poltrona>> GetPoltronasDisponiveisPorVooAsync(int vooId)
        {
            return await _dbSet
                .AsNoTracking()
                .Include(p => p.Voo)
                .Where(p => p.VooId == vooId && p.Disponivel)
                .OrderBy(p => p.NumeroPoltrona)
                .ToListAsync();
        }

        public async Task<Poltrona> GetPoltronaComVooAsync(int id)
        {
            return await _dbSet
                .Include(p => p.Voo)
                .FirstOrDefaultAsync(p => p.PoltronaId == id);
        }

        // =============================================
        // IMPLEMENTAÇÃO - VALIDAÇÕES
        // =============================================

        public async Task<bool> NumeroPoltronaExistsInVooAsync(int vooId, string numeroPoltrona)
        {
            return await _dbSet.AnyAsync(p =>
                p.VooId == vooId &&
                p.NumeroPoltrona == numeroPoltrona);
        }

        // =============================================
        // IMPLEMENTAÇÃO - ESTATÍSTICAS
        // =============================================

        public async Task<int> GetTotalPoltronasDisponiveisPorVooAsync(int vooId)
        {
            return await _dbSet.CountAsync(p =>
                p.VooId == vooId &&
                p.Disponivel);
        }

        public async Task<int> GetTotalPoltronasPorVooAsync(int vooId)
        {
            return await _dbSet.CountAsync(p => p.VooId == vooId);
        }
    }
}