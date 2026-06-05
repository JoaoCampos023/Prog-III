using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using SistemaAereo.Data.Context;
using SistemaAereo.Repositories.Interfaces;

namespace SistemaAereo.Repositories
{
    // Implementação genérica do repositório base
    // Fornece as operações básicas de CRUD para qualquer entidade T
    public class Repository<T> : IRepository<T> where T : class
    {
        // Contexto do banco de dados e DbSet da entidade
        protected readonly AirportsContext _context;
        protected readonly DbSet<T> _dbSet;

        // Construtor - recebe o contexto via injeção de dependência
        public Repository(AirportsContext context)
        {
            _context = context;
            _dbSet = context.Set<T>();
        }

        // =============================================
        // IMPLEMENTAÇÃO - OPERAÇÕES DE CONSULTA
        // =============================================

        // Busca uma entidade pelo ID
        public virtual async Task<T> GetByIdAsync(int id)
        {
            return await _dbSet.FindAsync(id);
        }

        // Busca todas as entidades
        public virtual async Task<IEnumerable<T>> GetAllAsync()
        {
            return await _dbSet.ToListAsync();
        }

        // Busca entidades que atendam a uma condição
        public virtual async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate)
        {
            return await _dbSet.Where(predicate).ToListAsync();
        }

        // Busca a primeira entidade que atenda a uma condição
        public virtual async Task<T> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate)
        {
            return await _dbSet.FirstOrDefaultAsync(predicate);
        }

        // Conta quantas entidades atendem a uma condição
        public virtual async Task<int> CountAsync(Expression<Func<T, bool>> predicate = null)
        {
            if (predicate == null)
                return await _dbSet.CountAsync();

            return await _dbSet.CountAsync(predicate);
        }

        // =============================================
        // IMPLEMENTAÇÃO - OPERAÇÕES DE ESCRITA
        // =============================================

        // Adiciona uma nova entidade
        public virtual async Task AddAsync(T entity)
        {
            await _dbSet.AddAsync(entity);
            await _context.SaveChangesAsync();
        }

        // Atualiza uma entidade existente
        public virtual async Task UpdateAsync(T entity)
        {
            _dbSet.Update(entity);
            await _context.SaveChangesAsync();
        }

        // Remove uma entidade
        public virtual async Task DeleteAsync(T entity)
        {
            _dbSet.Remove(entity);
            await _context.SaveChangesAsync();
        }

        // =============================================
        // IMPLEMENTAÇÃO - OPERAÇÕES DE VERIFICAÇÃO
        // =============================================

        // Verifica se existe alguma entidade que atenda a uma condição
        public virtual async Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate)
        {
            return await _dbSet.AnyAsync(predicate);
        }
    }
}