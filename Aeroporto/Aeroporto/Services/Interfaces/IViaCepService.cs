using SistemaAereo.Models.DTOs;

namespace SistemaAereo.Services.Interfaces
{
    // Interface para o serviço de integração com a API ViaCEP
    // Responsável por buscar endereços a partir do CEP
    public interface IViaCepService
    {
        // Busca endereço pelo CEP
        // Retorna os dados do endereço ou null se não encontrado
        Task<ViaCepResponseDto> GetAddressByZipCodeAsync(string zipCode);

        // Valida se o CEP é válido e existe
        Task<bool> IsZipCodeValidAsync(string zipCode);

        // Formata o CEP para o padrão 00000-000
        string FormatZipCode(string zipCode);

        // Remove formatação do CEP (deixa apenas números)
        string RemoveFormatting(string zipCode);
    }
}