using SistemaAereo.Data;
using SistemaAereo.Models;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using SistemaAereo.Repositories.Interfaces;

namespace SistemaAereo.Repositories
{
    public class VooRepository : Repository<Voo>, IVooRepository
    {
        private readonly AeroportoContext _context;

        // =============================================
        // CONSTRUTOR
        // =============================================

        public VooRepository(AeroportoContext context) : base(context)
        {
            _context = context;
        }

        // =============================================
        // IMPLEMENTAÇÃO - CONSULTAS COMPLEXAS COM INCLUDE
        // =============================================

        public async Task<IEnumerable<Voo>> GetVoosCompletosAsync()
        {
            return await _dbSet
                .AsNoTracking()
                .Include(v => v.AeroportoOrigem)
                .Include(v => v.AeroportoDestino)
                .Include(v => v.Aeronave)
                .Include(v => v.Escalas)
                    .ThenInclude(e => e.Aeroporto)
                .Include(v => v.Poltronas)
                .OrderBy(v => v.HorarioSaida)
                .ToListAsync();
        }

        public async Task<Voo> GetVooCompletoAsync(int id)
        {
            return await _dbSet
                .AsNoTracking()
                .Include(v => v.AeroportoOrigem)
                .Include(v => v.AeroportoDestino)
                .Include(v => v.Aeronave)
                .Include(v => v.Escalas)
                    .ThenInclude(e => e.Aeroporto)
                .Include(v => v.Poltronas)
                .FirstOrDefaultAsync(v => v.VooId == id);
        }

        public async Task<Voo> GetVooParaEdicaoAsync(int id)
        {
            return await _dbSet
                .Include(v => v.AeroportoOrigem)
                .Include(v => v.AeroportoDestino)
                .Include(v => v.Aeronave)
                .Include(v => v.Escalas)
                .Include(v => v.Poltronas)
                .FirstOrDefaultAsync(v => v.VooId == id);
        }

        // =============================================
        // IMPLEMENTAÇÃO - CONSULTAS FILTRADAS
        // =============================================

        public async Task<IEnumerable<Voo>> GetProximosVoosAsync(int quantidade = 5)
        {
            return await _dbSet
                .AsNoTracking()
                .Include(v => v.AeroportoOrigem)
                .Include(v => v.AeroportoDestino)
                .Include(v => v.Aeronave)
                .Where(v => v.HorarioSaida > DateTime.Now)
                .OrderBy(v => v.HorarioSaida)
                .Take(quantidade)
                .ToListAsync();
        }

        public async Task<IEnumerable<Voo>> GetVoosPorPeriodoAsync(DateTime inicio, DateTime fim)
        {
            return await _dbSet
                .AsNoTracking()
                .Include(v => v.AeroportoOrigem)
                .Include(v => v.AeroportoDestino)
                .Include(v => v.Aeronave)
                .Where(v => v.HorarioSaida >= inicio && v.HorarioSaida <= fim)
                .OrderBy(v => v.HorarioSaida)
                .ToListAsync();
        }

        public async Task<IEnumerable<Voo>> GetVoosPorAeroportoAsync(int aeroportoId)
        {
            return await _dbSet
                .AsNoTracking()
                .Include(v => v.AeroportoOrigem)
                .Include(v => v.AeroportoDestino)
                .Include(v => v.Aeronave)
                .Where(v => v.AeroportoOrigemId == aeroportoId || v.AeroportoDestinoId == aeroportoId)
                .OrderBy(v => v.HorarioSaida)
                .ToListAsync();
        }

        public async Task<IEnumerable<Voo>> GetVoosDisponiveisAsync()
        {
            return await _dbSet
                .AsNoTracking()
                .Include(v => v.AeroportoOrigem)
                .Include(v => v.AeroportoDestino)
                .Include(v => v.Aeronave)
                .Include(v => v.Poltronas)
                .Where(v => v.HorarioSaida > DateTime.Now &&
                           v.Poltronas.Any(p => p.Disponivel))
                .OrderBy(v => v.HorarioSaida)
                .ToListAsync();
        }

        // =============================================
        // IMPLEMENTAÇÃO - CONSULTAS COM FILTROS COMBINADOS
        // =============================================

        public async Task<IEnumerable<Voo>> GetVoosComFiltrosAsync(
            int? aeroportoOrigemId = null,
            int? aeroportoDestinoId = null,
            int? aeronaveId = null,
            DateTime? dataInicio = null,
            DateTime? dataFim = null,
            bool apenasComPoltronasDisponiveis = false)
        {
            var query = _dbSet
                .AsNoTracking()
                .Include(v => v.AeroportoOrigem)
                .Include(v => v.AeroportoDestino)
                .Include(v => v.Aeronave)
                .AsQueryable();

            if (aeroportoOrigemId.HasValue)
                query = query.Where(v => v.AeroportoOrigemId == aeroportoOrigemId.Value);

            if (aeroportoDestinoId.HasValue)
                query = query.Where(v => v.AeroportoDestinoId == aeroportoDestinoId.Value);

            if (aeronaveId.HasValue)
                query = query.Where(v => v.AeronaveId == aeronaveId.Value);

            if (dataInicio.HasValue)
                query = query.Where(v => v.HorarioSaida >= dataInicio.Value);

            if (dataFim.HasValue)
                query = query.Where(v => v.HorarioSaida <= dataFim.Value);

            if (apenasComPoltronasDisponiveis)
            {
                query = query.Include(v => v.Poltronas)
                            .Where(v => v.Poltronas.Any(p => p.Disponivel));
            }

            return await query
                .OrderBy(v => v.HorarioSaida)
                .ToListAsync();
        }

        // =============================================
        // IMPLEMENTAÇÃO - VALIDAÇÕES E VERIFICAÇÕES
        // =============================================

        public async Task<bool> NumeroVooExistsAsync(string numeroVoo, int? excludeId = null)
        {
            var query = _dbSet.AsNoTracking().Where(v => v.NumeroVoo == numeroVoo);

            if (excludeId.HasValue)
                query = query.Where(v => v.VooId != excludeId.Value);

            return await query.AnyAsync();
        }

        public async Task<bool> HasEscalasAsync(int vooId)
        {
            return await _context.Escalas
                .AsNoTracking()
                .AnyAsync(e => e.VooId == vooId);
        }

        public async Task<bool> HasPoltronasAsync(int vooId)
        {
            return await _context.Poltronas
                .AsNoTracking()
                .AnyAsync(p => p.VooId == vooId);
        }

        public async Task<bool> HasPoltronasOcupadasAsync(int vooId)
        {
            return await _context.Poltronas
                .AsNoTracking()
                .AnyAsync(p => p.VooId == vooId && !p.Disponivel);
        }

        // =============================================
        // IMPLEMENTAÇÃO - ESTATÍSTICAS E RELATÓRIOS
        // =============================================

        public async Task<int> GetTotalVoosAsync()
        {
            return await _dbSet.AsNoTracking().CountAsync();
        }

        public async Task<int> GetTotalVoosPorAeroportoAsync(int aeroportoId)
        {
            return await _dbSet
                .AsNoTracking()
                .CountAsync(v => v.AeroportoOrigemId == aeroportoId || v.AeroportoDestinoId == aeroportoId);
        }

        public async Task<int> GetTotalVoosPorPeriodoAsync(DateTime inicio, DateTime fim)
        {
            return await _dbSet
                .AsNoTracking()
                .CountAsync(v => v.HorarioSaida >= inicio && v.HorarioSaida <= fim);
        }

        public async Task<int> GetTotalPoltronasDisponiveisAsync(int vooId)
        {
            return await _context.Poltronas
                .AsNoTracking()
                .CountAsync(p => p.VooId == vooId && p.Disponivel);
        }

        public async Task<int> GetTotalPoltronasOcupadasAsync(int vooId)
        {
            return await _context.Poltronas
                .AsNoTracking()
                .CountAsync(p => p.VooId == vooId && !p.Disponivel);
        }

        // =============================================
        // IMPLEMENTAÇÃO - CONSULTAS ESPECIALIZADAS
        // =============================================

        public async Task<IEnumerable<Voo>> GetVoosHojeAsync()
        {
            var hoje = DateTime.Today;
            var amanha = hoje.AddDays(1);

            return await _dbSet
                .AsNoTracking()
                .Include(v => v.AeroportoOrigem)
                .Include(v => v.AeroportoDestino)
                .Include(v => v.Aeronave)
                .Where(v => v.HorarioSaida >= hoje && v.HorarioSaida < amanha)
                .OrderBy(v => v.HorarioSaida)
                .ToListAsync();
        }

        public async Task<IEnumerable<Voo>> GetVoosPorStatusAsync(string status)
        {
            var agora = DateTime.Now;
            var query = _dbSet
                .AsNoTracking()
                .Include(v => v.AeroportoOrigem)
                .Include(v => v.AeroportoDestino)
                .Include(v => v.Aeronave)
                .AsQueryable();

            return status?.ToLower() switch
            {
                "futuros" => await query.Where(v => v.HorarioSaida > agora)
                                  .OrderBy(v => v.HorarioSaida).ToListAsync(),
                "passados" => await query.Where(v => v.HorarioSaida <= agora)
                                   .OrderByDescending(v => v.HorarioSaida).ToListAsync(),
                "hoje" => await GetVoosHojeAsync(),
                _ => await query.OrderBy(v => v.HorarioSaida).ToListAsync()
            };
        }

        public async Task<Dictionary<string, int>> GetEstatisticasVoosPorAeroportoAsync()
        {
            var aeroportos = await _context.Aeroportos.ToListAsync();
            var estatisticas = new Dictionary<string, int>();

            foreach (var aeroporto in aeroportos)
            {
                var total = await GetTotalVoosPorAeroportoAsync(aeroporto.AeroportoId);
                estatisticas[aeroporto.Nome] = total;
            }

            return estatisticas;
        }

        // =============================================
        // IMPLEMENTAÇÃO - OPERAÇÕES EM LOTE
        // =============================================

        public async Task AtualizarStatusVoosAsync()
        {
            var voosAntigos = await _dbSet
                .Where(v => v.HorarioSaida < DateTime.Now.AddMonths(-6))
                .ToListAsync();

            // Lógica de atualização em lote pode ser implementada aqui
            foreach (var voo in voosAntigos)
            {
                // Operações em lote
            }

            await _context.SaveChangesAsync();
        }

        public async Task CancelarVoosComBaixaOcupacaoAsync(double percentualMinimo)
        {
            var voosFuturos = await _dbSet
                .Include(v => v.Poltronas)
                .Where(v => v.HorarioSaida > DateTime.Now)
                .ToListAsync();

            foreach (var voo in voosFuturos)
            {
                var totalPoltronas = voo.Poltronas.Count;
                var poltronasOcupadas = voo.Poltronas.Count(p => !p.Disponivel);
                var ocupacao = totalPoltronas > 0 ? (double)poltronasOcupadas / totalPoltronas : 0;

                if (ocupacao < percentualMinimo)
                {
                    // Lógica para cancelar voo
                }
            }

            await _context.SaveChangesAsync();
        }

        // =============================================
        // IMPLEMENTAÇÃO - CONSULTAS PAGINADAS
        // =============================================

        public async Task<(IEnumerable<Voo> Voos, int TotalCount)> GetVoosPaginadosAsync(
            int pagina = 1,
            int itensPorPagina = 10,
            string ordenacao = "data",
            bool ascendente = true)
        {
            var query = _dbSet
                .AsNoTracking()
                .Include(v => v.AeroportoOrigem)
                .Include(v => v.AeroportoDestino)
                .Include(v => v.Aeronave)
                .AsQueryable();

            query = ordenacao?.ToLower() switch
            {
                "numero" => ascendente ? query.OrderBy(v => v.NumeroVoo) : query.OrderByDescending(v => v.NumeroVoo),
                "origem" => ascendente ? query.OrderBy(v => v.AeroportoOrigem.Nome) : query.OrderByDescending(v => v.AeroportoOrigem.Nome),
                "destino" => ascendente ? query.OrderBy(v => v.AeroportoDestino.Nome) : query.OrderByDescending(v => v.AeroportoDestino.Nome),
                _ => ascendente ? query.OrderBy(v => v.HorarioSaida) : query.OrderByDescending(v => v.HorarioSaida)
            };

            var totalCount = await query.CountAsync();
            var voos = await query
                .Skip((pagina - 1) * itensPorPagina)
                .Take(itensPorPagina)
                .ToListAsync();

            return (voos, totalCount);
        }

        // =============================================
        // IMPLEMENTAÇÃO - OVERRIDE DOS MÉTODOS BASE
        // =============================================

        public override async Task<IEnumerable<Voo>> GetAllAsync()
        {
            return await _dbSet
                .AsNoTracking()
                .Include(v => v.AeroportoOrigem)
                .Include(v => v.AeroportoDestino)
                .Include(v => v.Aeronave)
                .OrderBy(v => v.HorarioSaida)
                .ToListAsync();
        }

        public override async Task<IEnumerable<Voo>> FindAsync(Expression<Func<Voo, bool>> predicate)
        {
            return await _dbSet
                .AsNoTracking()
                .Include(v => v.AeroportoOrigem)
                .Include(v => v.AeroportoDestino)
                .Include(v => v.Aeronave)
                .Where(predicate)
                .OrderBy(v => v.HorarioSaida)
                .ToListAsync();
        }
    }
}