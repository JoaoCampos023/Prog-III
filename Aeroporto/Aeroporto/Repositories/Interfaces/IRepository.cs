using System.Linq.Expressions;

namespace SistemaAereo.Repositories.Interfaces
{
    // Interface genérica para repositório base
    // Define as operações básicas de CRUD para qualquer entidade T
    public interface IRepository<T> where T : class
    {
        // =============================================
        // OPERAÇÕES DE CONSULTA
        // =============================================

        // Busca uma entidade pelo seu ID
        Task<T> GetByIdAsync(int id);

        // Busca todas as entidades
        Task<IEnumerable<T>> GetAllAsync();

        // Busca entidades que atendam a uma condição
        Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate);

        // Busca a primeira entidade que atenda a uma condição
        Task<T> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate);

        // Conta quantas entidades atendem a uma condição (ou todas se não informada)
        Task<int> CountAsync(Expression<Func<T, bool>> predicate = null);

        // =============================================
        // OPERAÇÕES DE ESCRITA
        // =============================================

        // Adiciona uma nova entidade
        Task AddAsync(T entity);

        // Atualiza uma entidade existente
        Task UpdateAsync(T entity);

        // Remove uma entidade
        Task DeleteAsync(T entity);

        // =============================================
        // OPERAÇÕES DE VERIFICAÇÃO
        // =============================================

        // Verifica se existe alguma entidade que atenda a uma condição
        Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate);
    }
}