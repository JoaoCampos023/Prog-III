using System.Text.Json;
using SistemaAereo.Models.DTOs;
using SistemaAereo.Services.Interfaces;

namespace SistemaAereo.Services
{
    // Implementação do serviço de integração com a API ViaCEP
    public class ViaCepService : IViaCepService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<ViaCepService> _logger;

        public ViaCepService(HttpClient httpClient, ILogger<ViaCepService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        // Busca endereço pelo CEP
        public async Task<ViaCepResponseDto> GetAddressByZipCodeAsync(string zipCode)
        {
            try
            {
                // Remove caracteres não numéricos (espaços, hífens, etc.)
                var cleanZipCode = RemoveFormatting(zipCode);

                // Valida o formato do CEP (deve ter exatamente 8 dígitos)
                if (string.IsNullOrEmpty(cleanZipCode) || cleanZipCode.Length != 8 || !cleanZipCode.All(char.IsDigit))
                {
                    _logger.LogWarning($"CEP inválido: {zipCode}");
                    return null;
                }

                // URL da API ViaCEP
                var url = $"https://viacep.com.br/ws/{cleanZipCode}/json/";
                _logger.LogInformation($"Consultando CEP: {cleanZipCode}");

                var response = await _httpClient.GetAsync(url);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning($"Erro ao consultar CEP {cleanZipCode}: {response.StatusCode}");
                    return null;
                }

                var json = await response.Content.ReadAsStringAsync();
                var address = JsonSerializer.Deserialize<ViaCepResponseDto>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                // Verifica se o CEP foi encontrado (campo "erro" é true quando não encontrado)
                if (address == null || address.Error)
                {
                    _logger.LogWarning($"CEP {cleanZipCode} não encontrado");
                    return null;
                }

                _logger.LogInformation($"CEP {cleanZipCode} encontrado: {address.Street}, {address.City}/{address.State}");
                return address;
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, $"Erro de rede ao consultar CEP {zipCode}");
                return null;
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, $"Erro ao deserializar resposta do CEP {zipCode}");
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Erro inesperado ao consultar CEP {zipCode}");
                return null;
            }
        }

        // Valida se o CEP é válido e existe
        public async Task<bool> IsZipCodeValidAsync(string zipCode)
        {
            var address = await GetAddressByZipCodeAsync(zipCode);
            return address != null && address.IsValid;
        }

        // Formata o CEP para o padrão 00000-000
        public string FormatZipCode(string zipCode)
        {
            var cleanZipCode = RemoveFormatting(zipCode);
            if (string.IsNullOrEmpty(cleanZipCode) || cleanZipCode.Length != 8)
                return zipCode;

            return $"{cleanZipCode.Substring(0, 5)}-{cleanZipCode.Substring(5, 3)}";
        }

        // Remove formatação do CEP (deixa apenas números)
        public string RemoveFormatting(string zipCode)
        {
            if (string.IsNullOrEmpty(zipCode))
                return string.Empty;

            return new string(zipCode.Where(char.IsDigit).ToArray());
        }
    }
}