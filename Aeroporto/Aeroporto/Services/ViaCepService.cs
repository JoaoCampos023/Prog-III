using System.Text.Json;
using SistemaAereo.Models.DTOs;
using SistemaAereo.Services.Interfaces;

namespace SistemaAereo.Services
{
    public class ViaCepService : IViaCepService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<ViaCepService> _logger;

        public ViaCepService(HttpClient httpClient, ILogger<ViaCepService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public async Task<ViaCepResponseDto> BuscarEnderecoPorCepAsync(string cep)
        {
            try
            {
                // Limpar formatação do CEP
                var cepLimpo = RemoverFormatacao(cep);

                // Validar formato (8 dígitos)
                if (string.IsNullOrEmpty(cepLimpo) || cepLimpo.Length != 8 || !cepLimpo.All(char.IsDigit))
                {
                    _logger.LogWarning($"CEP inválido: {cep}");
                    return null;
                }

                // URL da API ViaCEP
                var url = $"https://viacep.com.br/ws/{cepLimpo}/json/";

                _logger.LogInformation($"Consultando CEP: {cepLimpo}");

                var response = await _httpClient.GetAsync(url);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning($"Erro ao consultar CEP {cepLimpo}: {response.StatusCode}");
                    return null;
                }

                var json = await response.Content.ReadAsStringAsync();
                var endereco = JsonSerializer.Deserialize<ViaCepResponseDto>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (endereco == null || endereco.Erro)
                {
                    _logger.LogWarning($"CEP {cepLimpo} não encontrado");
                    return null;
                }

                _logger.LogInformation($"CEP {cepLimpo} encontrado: {endereco.Logradouro}, {endereco.Localidade}/{endereco.Uf}");

                return endereco;
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, $"Erro de rede ao consultar CEP {cep}");
                return null;
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, $"Erro ao deserializar resposta do CEP {cep}");
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Erro inesperado ao consultar CEP {cep}");
                return null;
            }
        }

        public async Task<bool> CepIsValidAsync(string cep)
        {
            var endereco = await BuscarEnderecoPorCepAsync(cep);
            return endereco != null && endereco.IsValid;
        }

        public string FormatarCep(string cep)
        {
            var cepLimpo = RemoverFormatacao(cep);
            if (string.IsNullOrEmpty(cepLimpo) || cepLimpo.Length != 8)
                return cep;

            return $"{cepLimpo.Substring(0, 5)}-{cepLimpo.Substring(5, 3)}";
        }

        public string RemoverFormatacao(string cep)
        {
            if (string.IsNullOrEmpty(cep))
                return string.Empty;

            return new string(cep.Where(char.IsDigit).ToArray());
        }
    }
}