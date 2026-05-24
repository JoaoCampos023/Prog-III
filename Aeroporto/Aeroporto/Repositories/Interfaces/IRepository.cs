using System.Linq.Expressions;

namespace SistemaAereo.Repositories.Interfaces
{
    public interface IRepository<T> where T : class
    {
        // =============================================
        // OPERAÇÕES BÁSICAS DE CRUD
        // =============================================

        Task<T> GetByIdAsync(int id);
        Task<IEnumerable<T>> GetAllAsync();
        Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate);
        Task AddAsync(T entity);
        Task UpdateAsync(T entity);
        Task DeleteAsync(T entity);

        // =============================================
        // OPERAÇÕES DE CONSULTA AVANÇADAS
        // =============================================

        Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate);
        Task<T> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate);
        Task<int> CountAsync(Expression<Func<T, bool>> predicate = null);
    }
}