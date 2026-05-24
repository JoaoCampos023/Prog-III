using Microsoft.EntityFrameworkCore;
using SistemaAereo.Data.Context;
using SistemaAereo.Models.Entities;
using SistemaAereo.Repositories.Interfaces;

namespace SistemaAereo.Repositories
{
    public class AeroportoRepository : Repository<Aeroporto>, IAeroportoRepository
    {
        // =============================================
        // CONSTRUTOR
        // =============================================

        public AeroportoRepository(AeroportoContext context) : base(context) { }

        // =============================================
        // IMPLEMENTAÇÃO - VALIDAÇÕES DE UNICIDADE
        // =============================================

        public async Task<bool> CodigoIATAExistsAsync(string codigoIATA, int? excludeId = null)
        {
            if (excludeId.HasValue)
                return await _dbSet.AnyAsync(a =>
                    a.CodigoIATA == codigoIATA &&
                    a.AeroportoId != excludeId.Value);

            return await _dbSet.AnyAsync(a => a.CodigoIATA == codigoIATA);
        }

        // =============================================
        // IMPLEMENTAÇÃO - VALIDAÇÕES DE DEPENDÊNCIA
        // =============================================

        public async Task<bool> HasVoosAsync(int aeroportoId)
        {
            return await _context.Voos.AnyAsync(v =>
                v.AeroportoOrigemId == aeroportoId ||
                v.AeroportoDestinoId == aeroportoId);
        }
    }
}