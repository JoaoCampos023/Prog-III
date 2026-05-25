using SistemaAereo.Models.DTOs;
using SistemaAereo.Models.Entities;

namespace SistemaAereo.Facades.Interfaces
{
    /// <summary>
    /// Fachada para operações complexas relacionadas a voos
    /// </summary>
    public interface IVooFacade
    {
        /// <summary>
        /// Cria um novo voo com todas as dependências
        /// </summary>
        Task<VooResultDto> CriarVooAsync(Voo voo);

        /// <summary>
        /// Atualiza um voo existente
        /// </summary>
        Task<VooResultDto> AtualizarVooAsync(Voo voo);

        /// <summary>
        /// Exclui um voo e todas suas dependências
        /// </summary>
        Task<VooResultDto> ExcluirVooAsync(int vooId);

        /// <summary>
        /// Recria as poltronas de um voo
        /// </summary>
        Task<VooResultDto> RecriarPoltronasAsync(int vooId);

        /// <summary>
        /// Obtém estatísticas completas de um voo
        /// </summary>
        Task<VooEstatisticasDto> ObterEstatisticasVooAsync(int vooId);
    }
}