using System.ComponentModel.DataAnnotations;

namespace SistemaAereo.Models.DTOs
{
    public class EmitirPassagemRequestDto
    {
        [Required(ErrorMessage = "Cliente é obrigatório")]
        public int ClienteId { get; set; }

        [Required(ErrorMessage = "Voo é obrigatório")]
        public int VooId { get; set; }

        [Required(ErrorMessage = "Poltrona é obrigatória")]
        public int PoltronaId { get; set; }
    }
}