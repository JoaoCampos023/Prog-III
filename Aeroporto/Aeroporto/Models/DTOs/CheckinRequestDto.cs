namespace SistemaAereo.Models.DTOs
{
    public class CheckinRequestDto
    {
        public int PassagemId { get; set; }
        public int NumeroBagagens { get; set; }
        public decimal PesoBagagens { get; set; }
    }
}