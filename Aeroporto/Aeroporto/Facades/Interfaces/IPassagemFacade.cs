using SistemaAereo.Models.DTOs;
using SistemaAereo.Models.Entities;

namespace SistemaAereo.Facades.Interfaces
{
    /// <summary>
    /// Fachada para operações complexas relacionadas a passagens
    /// </summary>
    public interface IPassagemFacade
    {
        /// <summary>
        /// Emite uma nova passagem
        /// </summary>
        Task<PassagemResultDto> EmitirPassagemAsync(EmitirPassagemRequestDto request);

        /// <summary>
        /// Cancela uma passagem existente
        /// </summary>
        Task<PassagemResultDto> CancelarPassagemAsync(CancelarPassagemRequestDto request);

        /// <summary>
        /// Realiza check-in de uma passagem
        /// </summary>
        Task<PassagemResultDto> RealizarCheckinAsync(CheckinRequestDto request);

        /// <summary>
        /// Registra embarque de uma passagem
        /// </summary>
        Task<PassagemResultDto> RegistrarEmbarqueAsync(int passagemId);

        /// <summary>
        /// Obtém detalhes completos de uma passagem
        /// </summary>
        Task<Passagem> ObterPassagemCompletaAsync(int passagemId);

        /// <summary>
        /// Verifica se uma poltrona está disponível
        /// </summary>
        Task<bool> VerificarDisponibilidadePoltronaAsync(int vooId, int poltronaId);
    }
}