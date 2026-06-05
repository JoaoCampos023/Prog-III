using System.Text.Json.Serialization;

namespace SistemaAereo.Models.DTOs
{
    // DTO para resposta da API ViaCEP
    public class ViaCepResponseDto
    {
        // CEP formatado (ex: 01001-000)
        [JsonPropertyName("cep")]
        public string ZipCode { get; set; }

        // Nome da rua/avenida/logradouro
        [JsonPropertyName("logradouro")]
        public string Street { get; set; }

        // Complemento do endereço (se houver)
        [JsonPropertyName("complemento")]
        public string Complement { get; set; }

        // Nome do bairro
        [JsonPropertyName("bairro")]
        public string Neighborhood { get; set; }

        // Nome da cidade
        [JsonPropertyName("localidade")]
        public string City { get; set; }

        // Sigla do estado (UF)
        [JsonPropertyName("uf")]
        public string State { get; set; }

        // Código IBGE da cidade
        [JsonPropertyName("ibge")]
        public string Ibge { get; set; }

        // Código GIA (opcional)
        [JsonPropertyName("gia")]
        public string Gia { get; set; }

        // Código DDD da região
        [JsonPropertyName("ddd")]
        public string Ddd { get; set; }

        // Código SIAFI (opcional)
        [JsonPropertyName("siafi")]
        public string Siafi { get; set; }

        // Indica se houve erro na consulta
        [JsonPropertyName("erro")]
        public bool Error { get; set; }

        // Indica se o CEP é válido e existe
        public bool IsValid => !Error && !string.IsNullOrEmpty(ZipCode);
    }
}