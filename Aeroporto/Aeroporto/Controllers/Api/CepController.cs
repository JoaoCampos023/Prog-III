using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SistemaAereo.Services.Interfaces;

namespace SistemaAereo.Controllers.Api
{
    // Controller da API para consulta de CEP
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class CepController : ControllerBase
    {
        private readonly IViaCepService _viaCepService;
        private readonly ILogger<CepController> _logger;

        public CepController(IViaCepService viaCepService, ILogger<CepController> logger)
        {
            _viaCepService = viaCepService;
            _logger = logger;
        }

        // Busca endereço por CEP
        [HttpGet("{zipCode}")]
        public async Task<IActionResult> GetAddressByZipCode(string zipCode)
        {
            // Validação do parâmetro
            if (string.IsNullOrEmpty(zipCode))
            {
                return BadRequest(new { success = false, message = "CEP é obrigatório" });
            }

            var address = await _viaCepService.GetAddressByZipCodeAsync(zipCode);

            if (address == null || !address.IsValid)
            {
                return NotFound(new { success = false, message = "CEP não encontrado" });
            }

            return Ok(new
            {
                success = true,
                data = new
                {
                    zipCode = address.ZipCode,
                    street = address.Street,
                    complement = address.Complement,
                    neighborhood = address.Neighborhood,
                    city = address.City,
                    state = address.State,
                    ddd = address.Ddd
                }
            });
        }

        // Valida se o CEP existe
        [HttpGet("validate/{zipCode}")]
        public async Task<IActionResult> ValidateZipCode(string zipCode)
        {
            var isValid = await _viaCepService.IsZipCodeValidAsync(zipCode);

            return Ok(new
            {
                success = true,
                zipCode = _viaCepService.FormatZipCode(zipCode),
                isValid = isValid
            });
        }
    }
}