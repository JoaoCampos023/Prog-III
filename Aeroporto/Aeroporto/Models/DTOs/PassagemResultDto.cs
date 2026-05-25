using SistemaAereo.Models.Entities;

namespace SistemaAereo.Models.DTOs
{
    public class PassagemResultDto
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public string ErrorMessage { get; set; }
        public Passagem Passagem { get; set; }
        public int PassagemId { get; set; }
        public string NumeroBilhete { get; set; }

        public static PassagemResultDto Ok(Passagem passagem)
        {
            return new PassagemResultDto
            {
                Success = true,
                Message = "Operação realizada com sucesso",
                Passagem = passagem,
                PassagemId = passagem.PassagemId,
                NumeroBilhete = passagem.NumeroBilhete
            };
        }

        public static PassagemResultDto Fail(string errorMessage)
        {
            return new PassagemResultDto
            {
                Success = false,
                ErrorMessage = errorMessage,
                Message = "Falha na operação"
            };
        }

        public static PassagemResultDto CancelOk()
        {
            return new PassagemResultDto
            {
                Success = true,
                Message = "Passagem cancelada com sucesso"
            };
        }

        public static PassagemResultDto CheckinOk()
        {
            return new PassagemResultDto
            {
                Success = true,
                Message = "Check-in realizado com sucesso"
            };
        }

        public static PassagemResultDto EmbarqueOk()
        {
            return new PassagemResultDto
            {
                Success = true,
                Message = "Embarque registrado com sucesso"
            };
        }
    }
}