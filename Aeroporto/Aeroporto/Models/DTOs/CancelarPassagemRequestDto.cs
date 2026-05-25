namespace SistemaAereo.Models.DTOs
{
    public class CancelarPassagemRequestDto
    {
        public int PassagemId { get; set; }
        public string Motivo { get; set; }
    }
}