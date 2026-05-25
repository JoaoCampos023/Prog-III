using SistemaAereo.Models.DTOs;

namespace SistemaAereo.Services.Interfaces
{
    public interface IViaCepService
    {
        /// <summary>
        /// Busca endereço pelo CEP
        /// </summary>
        /// <param name="cep">CEP no formato 00000000 ou 00000-000</param>
        /// <returns>Dados do endereço ou null se não encontrado</returns>
        Task<ViaCepResponseDto> BuscarEnderecoPorCepAsync(string cep);

        /// <summary>
        /// Valida se o CEP é válido (formato e existência)
        /// </summary>
        Task<bool> CepIsValidAsync(string cep);

        /// <summary>
        /// Formata o CEP para o padrão 00000-000
        /// </summary>
        string FormatarCep(string cep);

        /// <summary>
        /// Remove formatação do CEP (00000-000 -> 00000000)
        /// </summary>
        string RemoverFormatacao(string cep);
    }
}