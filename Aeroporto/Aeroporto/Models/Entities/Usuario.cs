using Microsoft.AspNetCore.Identity;

namespace SistemaAereo.Models.Entities
{
    public class Usuario : IdentityUser
    {
        public string? NomeCompleto { get; set; }
        public DateTime DataCadastro { get; set; } = DateTime.Now;
        public bool Ativo { get; set; } = true;
    }
}