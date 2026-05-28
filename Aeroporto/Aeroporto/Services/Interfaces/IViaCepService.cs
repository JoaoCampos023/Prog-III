using SistemaAereo.Models.DTOs;

namespace SistemaAereo.Services.Interfaces
{
    public interface IViaCepService
    {
        /// <summary>
        /// Busca endereço pelo CEP
        /// </summary>
        Task<ViaCepResponseDto> GetAddressByZipCodeAsync(string zipCode);

        /// <summary>
        /// Valida se o CEP é válido
        /// </summary>
        Task<bool> IsZipCodeValidAsync(string zipCode);

        /// <summary>
        /// Formata o CEP para o padrão 00000-000
        /// </summary>
        string FormatZipCode(string zipCode);

        /// <summary>
        /// Remove formatação do CEP
        /// </summary>
        string RemoveFormatting(string zipCode);
    }
}