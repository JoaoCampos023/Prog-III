using Microsoft.AspNetCore.Mvc;
using SistemaAereo.Services.Interfaces;

namespace SistemaAereo.Controllers.Api
{
    [Route("api/[controller]")]
    [ApiController]
    public class CepController : ControllerBase
    {
        private readonly IViaCepService _viaCepService;
        private readonly ILogger<CepController> _logger;

        public CepController(IViaCepService viaCepService, ILogger<CepController> logger)
        {
            _viaCepService = viaCepService;
            _logger = logger;
        }

        /// <summary>
        /// Busca endereço por CEP
        /// </summary>
        /// <param name="cep">CEP no formato 00000000 ou 00000-000</param>
        /// <returns>Dados do endereço</returns>
        [HttpGet("{cep}")]
        public async Task<IActionResult> GetEnderecoByCep(string cep)
        {
            if (string.IsNullOrEmpty(cep))
            {
                return BadRequest(new { success = false, message = "CEP é obrigatório" });
            }

            var endereco = await _viaCepService.BuscarEnderecoPorCepAsync(cep);

            if (endereco == null || !endereco.IsValid)
            {
                return NotFound(new { success = false, message = "CEP não encontrado" });
            }

            return Ok(new
            {
                success = true,
                data = new
                {
                    cep = endereco.Cep,
                    logradouro = endereco.Logradouro,
                    complemento = endereco.Complemento,
                    bairro = endereco.Bairro,
                    cidade = endereco.Localidade,
                    uf = endereco.Uf,
                    ddd = endereco.Ddd
                }
            });
        }

        /// <summary>
        /// Valida se o CEP existe
        /// </summary>
        [HttpGet("validar/{cep}")]
        public async Task<IActionResult> ValidarCep(string cep)
        {
            var isValid = await _viaCepService.CepIsValidAsync(cep);

            return Ok(new
            {
                success = true,
                cep = _viaCepService.FormatarCep(cep),
                isValid = isValid
            });
        }
    }
}