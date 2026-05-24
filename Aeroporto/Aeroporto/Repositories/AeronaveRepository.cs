using Microsoft.EntityFrameworkCore;
using SistemaAereo.Data;
using SistemaAereo.Models;
using SistemaAereo.Repositories;
using SistemaAereo.Repositories.Interfaces;

public class AeronaveRepository : Repository<Aeronave>, IAeronaveRepository
{
    // =============================================
    // CONSTRUTOR
    // =============================================

    public AeronaveRepository(AeroportoContext context) : base(context) { }

    // =============================================
    // IMPLEMENTAÇÃO - CONSULTAS DE AERONAVES
    // =============================================

    public async Task<IEnumerable<Aeronave>> GetAeronavesComVoosAsync()
    {
        return await _dbSet
            .Include(a => a.Voos)
                .ThenInclude(v => v.AeroportoOrigem)
            .Include(a => a.Voos)
                .ThenInclude(v => v.AeroportoDestino)
            .OrderBy(a => a.TipoAeronave)
            .ToListAsync();
    }

    // =============================================
    // IMPLEMENTAÇÃO - VALIDAÇÕES DE DEPENDÊNCIA
    // =============================================

    public async Task<bool> HasVoosAsync(int aeronaveId)
    {
        return await _context.Voos.AnyAsync(v => v.AeronaveId == aeronaveId);
    }
}