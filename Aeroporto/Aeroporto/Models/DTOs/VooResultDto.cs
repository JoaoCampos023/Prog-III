using SistemaAereo.Models.Entities;

namespace SistemaAereo.Models.DTOs
{
    public class VooResultDto
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public string ErrorMessage { get; set; }
        public Voo Voo { get; set; }
        public int VooId { get; set; }

        public static VooResultDto Ok(Voo voo, string message = "Operação realizada com sucesso")
        {
            return new VooResultDto
            {
                Success = true,
                Message = message,
                Voo = voo,
                VooId = voo.VooId
            };
        }

        public static VooResultDto Fail(string errorMessage)
        {
            return new VooResultDto
            {
                Success = false,
                ErrorMessage = errorMessage,
                Message = "Falha na operação"
            };
        }
    }

    public class VooEstatisticasDto
    {
        public int TotalPoltronas { get; set; }
        public int PoltronasDisponiveis { get; set; }
        public int PoltronasOcupadas { get; set; }
        public int TotalPassagens { get; set; }
        public decimal FaturamentoTotal { get; set; }
        public double PercentualOcupacao { get; set; }
    }
}