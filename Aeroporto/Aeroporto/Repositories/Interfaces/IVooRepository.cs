using SistemaAereo.Models.Entities;
using System.Linq.Expressions;

namespace SistemaAereo.Repositories.Interfaces
{
    public interface IVooRepository : IRepository<Voo>
    {
        // =============================================
        // CONSULTAS COMPLEXAS COM INCLUDE
        // =============================================

        Task<IEnumerable<Voo>> GetVoosCompletosAsync();
        Task<Voo> GetVooCompletoAsync(int id);
        Task<Voo> GetVooParaEdicaoAsync(int id);

        // =============================================
        // CONSULTAS FILTRADAS
        // =============================================

        Task<IEnumerable<Voo>> GetProximosVoosAsync(int quantidade = 5);
        Task<IEnumerable<Voo>> GetVoosPorPeriodoAsync(DateTime inicio, DateTime fim);
        Task<IEnumerable<Voo>> GetVoosPorAeroportoAsync(int aeroportoId);
        Task<IEnumerable<Voo>> GetVoosDisponiveisAsync();

        // =============================================
        // CONSULTAS COM FILTROS COMBINADOS
        // =============================================

        Task<IEnumerable<Voo>> GetVoosComFiltrosAsync(
            int? aeroportoOrigemId = null,
            int? aeroportoDestinoId = null,
            int? aeronaveId = null,
            DateTime? dataInicio = null,
            DateTime? dataFim = null,
            bool apenasComPoltronasDisponiveis = false);

        // =============================================
        // VALIDAÇÕES E VERIFICAÇÕES
        // =============================================

        Task<bool> NumeroVooExistsAsync(string numeroVoo, int? excludeId = null);
        Task<bool> HasEscalasAsync(int vooId);
        Task<bool> HasPoltronasAsync(int vooId);
        Task<bool> HasPoltronasOcupadasAsync(int vooId);

        // =============================================
        // ESTATÍSTICAS E RELATÓRIOS
        // =============================================

        Task<int> GetTotalVoosAsync();
        Task<int> GetTotalVoosPorAeroportoAsync(int aeroportoId);
        Task<int> GetTotalVoosPorPeriodoAsync(DateTime inicio, DateTime fim);
        Task<int> GetTotalPoltronasDisponiveisAsync(int vooId);
        Task<int> GetTotalPoltronasOcupadasAsync(int vooId);

        // =============================================
        // CONSULTAS ESPECIALIZADAS PARA DASHBOARD
        // =============================================

        Task<IEnumerable<Voo>> GetVoosHojeAsync();
        Task<IEnumerable<Voo>> GetVoosPorStatusAsync(string status);
        Task<Dictionary<string, int>> GetEstatisticasVoosPorAeroportoAsync();

        // =============================================
        // OPERAÇÕES EM LOTE
        // =============================================

        Task AtualizarStatusVoosAsync();
        Task CancelarVoosComBaixaOcupacaoAsync(double percentualMinimo);

        // =============================================
        // CONSULTAS PAGINADAS
        // =============================================

        Task<(IEnumerable<Voo> Voos, int TotalCount)> GetVoosPaginadosAsync(
            int pagina = 1,
            int itensPorPagina = 10,
            string ordenacao = "data",
            bool ascendente = true);

        // =============================================
        // OVERRIDE DOS MÉTODOS BASE
        // =============================================

        new Task<IEnumerable<Voo>> GetAllAsync();
        new Task<IEnumerable<Voo>> FindAsync(Expression<Func<Voo, bool>> predicate);
    }
}