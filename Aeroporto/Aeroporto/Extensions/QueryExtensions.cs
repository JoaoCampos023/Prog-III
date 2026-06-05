using Microsoft.EntityFrameworkCore;

namespace SistemaAereo.Extensions
{
    public static class QueryExtensions
    {

        /// Aplica AsNoTracking padronizado para consultas de leitura
        public static IQueryable<T> AsReadOnly<T>(this IQueryable<T> query) where T : class
        {
            return query.AsNoTracking();
        }

        /// Obtém query para operações de escrita (com tracking)
        public static IQueryable<T> AsWritable<T>(this DbSet<T> dbSet) where T : class
        {
            return dbSet.AsQueryable();
        }
    }
}