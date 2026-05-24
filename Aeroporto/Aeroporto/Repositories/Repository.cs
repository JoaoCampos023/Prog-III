using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using SistemaAereo.Data;
using SistemaAereo.Repositories.Interfaces;

namespace SistemaAereo.Repositories
{
    public class Repository<T> : IRepository<T> where T : class
    {
        protected readonly AeroportoContext _context;
        protected readonly DbSet<T> _dbSet;

        // =============================================
        // CONSTRUTOR
        // =============================================

        public Repository(AeroportoContext context)
        {
            _context = context;
            _dbSet = context.Set<T>();
        }

        // =============================================
        // IMPLEMENTAÇÃO - OPERAÇÕES DE CONSULTA
        // =============================================

        public virtual async Task<T> GetByIdAsync(int id)
        {
            return await _dbSet.FindAsync(id);
        }

        public virtual async Task<IEnumerable<T>> GetAllAsync()
        {
            return await _dbSet.ToListAsync();
        }

        public virtual async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate)
        {
            return await _dbSet.Where(predicate).ToListAsync();
        }

        public virtual async Task<T> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate)
        {
            return await _dbSet.FirstOrDefaultAsync(predicate);
        }

        public virtual async Task<int> CountAsync(Expression<Func<T, bool>> predicate = null)
        {
            if (predicate == null)
                return await _dbSet.CountAsync();

            return await _dbSet.CountAsync(predicate);
        }

        // =============================================
        // IMPLEMENTAÇÃO - OPERAÇÕES DE ESCRITA
        // =============================================

        public virtual async Task AddAsync(T entity)
        {
            await _dbSet.AddAsync(entity);
            await _context.SaveChangesAsync();
        }

        public virtual async Task UpdateAsync(T entity)
        {
            _dbSet.Update(entity);
            await _context.SaveChangesAsync();
        }

        public virtual async Task DeleteAsync(T entity)
        {
            _dbSet.Remove(entity);
            await _context.SaveChangesAsync();
        }

        // =============================================
        // IMPLEMENTAÇÃO - OPERAÇÕES DE VERIFICAÇÃO
        // =============================================

        public virtual async Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate)
        {
            return await _dbSet.AnyAsync(predicate);
        }
    }
}