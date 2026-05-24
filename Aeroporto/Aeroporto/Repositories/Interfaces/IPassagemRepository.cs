using Microsoft.EntityFrameworkCore;
using SistemaAereo.Data;
using SistemaAereo.Models;
using SistemaAereo.Repositories.Interfaces;

namespace SistemaAereo.Repositories
{
    public interface IPassagemRepository : IRepository<Passagem>
    {
        // =============================================
        // CONSULTAS COMPLEXAS COM INCLUDE
        // =============================================

        Task<IEnumerable<Passagem>> GetPassagensCompletasAsync();
        Task<Passagem> GetPassagemCompletaAsync(int id);

        // =============================================
        // CONSULTAS FILTRADAS
        // =============================================

        Task<IEnumerable<Passagem>> GetPassagensPorClienteAsync(int clienteId);
        Task<IEnumerable<Passagem>> GetPassagensPorVooAsync(int vooId);
        Task<IEnumerable<Passagem>> GetPassagensPorPeriodoAsync(DateTime inicio, DateTime fim);

        // =============================================
        // VALIDAÇÕES E VERIFICAÇÕES
        // =============================================

        Task<bool> NumeroBilheteExistsAsync(string numeroBilhete);
        Task<bool> PoltronaOcupadaAsync(int vooId, int poltronaId);

        // =============================================
        // ESTATÍSTICAS E RELATÓRIOS
        // =============================================

        Task<int> GetTotalPassagensVendidasPorVooAsync(int vooId);
        Task<decimal> GetFaturamentoPorVooAsync(int vooId);
    }

    // =============================================
    // IMPLEMENTAÇÃO DO REPOSITÓRIO
    // =============================================

    public class PassagemRepository : Repository<Passagem>, IPassagemRepository
    {
        public PassagemRepository(AeroportoContext context) : base(context) { }

        // =============================================
        // IMPLEMENTAÇÃO - CONSULTAS COMPLEXAS
        // =============================================

        public async Task<IEnumerable<Passagem>> GetPassagensCompletasAsync()
        {
            return await _dbSet
                .AsNoTracking()
                .Include(p => p.Voo)
                    .ThenInclude(v => v.AeroportoOrigem)
                .Include(p => p.Voo)
                    .ThenInclude(v => v.AeroportoDestino)
                .Include(p => p.Cliente)
                .Include(p => p.Poltrona)
                .OrderByDescending(p => p.DataEmissao)
                .ToListAsync();
        }

        public async Task<Passagem> GetPassagemCompletaAsync(int id)
        {
            return await _dbSet
                .Include(p => p.Voo)
                    .ThenInclude(v => v.AeroportoOrigem)
                .Include(p => p.Voo)
                    .ThenInclude(v => v.AeroportoDestino)
                .Include(p => p.Voo)
                    .ThenInclude(v => v.Aeronave)
                .Include(p => p.Cliente)
                .Include(p => p.Poltrona)
                .FirstOrDefaultAsync(p => p.PassagemId == id);
        }

        // =============================================
        // IMPLEMENTAÇÃO - CONSULTAS FILTRADAS
        // =============================================

        public async Task<IEnumerable<Passagem>> GetPassagensPorClienteAsync(int clienteId)
        {
            return await _dbSet
                .AsNoTracking()
                .Include(p => p.Voo)
                    .ThenInclude(v => v.AeroportoOrigem)
                .Include(p => p.Voo)
                    .ThenInclude(v => v.AeroportoDestino)
                .Include(p => p.Poltrona)
                .Where(p => p.ClienteId == clienteId)
                .OrderByDescending(p => p.DataEmissao)
                .ToListAsync();
        }

        public async Task<IEnumerable<Passagem>> GetPassagensPorVooAsync(int vooId)
        {
            return await _dbSet
                .AsNoTracking()
                .Include(p => p.Cliente)
                .Include(p => p.Poltrona)
                .Where(p => p.VooId == vooId)
                .OrderBy(p => p.Poltrona.NumeroPoltrona)
                .ToListAsync();
        }

        public async Task<IEnumerable<Passagem>> GetPassagensPorPeriodoAsync(DateTime inicio, DateTime fim)
        {
            return await _dbSet
                .AsNoTracking()
                .Include(p => p.Voo)
                .Include(p => p.Cliente)
                .Where(p => p.DataEmissao >= inicio && p.DataEmissao <= fim)
                .OrderByDescending(p => p.DataEmissao)
                .ToListAsync();
        }

        // =============================================
        // IMPLEMENTAÇÃO - VALIDAÇÕES
        // =============================================

        public async Task<bool> NumeroBilheteExistsAsync(string numeroBilhete)
        {
            return await _dbSet.AnyAsync(p => p.NumeroBilhete == numeroBilhete);
        }

        public async Task<bool> PoltronaOcupadaAsync(int vooId, int poltronaId)
        {
            return await _dbSet.AnyAsync(p =>
                p.VooId == vooId &&
                p.PoltronaId == poltronaId &&
                p.Status != "Cancelada");
        }

        // =============================================
        // IMPLEMENTAÇÃO - ESTATÍSTICAS
        // =============================================

        public async Task<int> GetTotalPassagensVendidasPorVooAsync(int vooId)
        {
            return await _dbSet.CountAsync(p =>
                p.VooId == vooId &&
                p.Status != "Cancelada");
        }

        public async Task<decimal> GetFaturamentoPorVooAsync(int vooId)
        {
            return await _dbSet
                .Where(p => p.VooId == vooId && p.Status != "Cancelada")
                .SumAsync(p => p.Preco);
        }
    }
}