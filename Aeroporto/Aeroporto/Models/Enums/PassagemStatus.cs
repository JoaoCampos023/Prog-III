namespace SistemaAereo.Models.Enums
{
    public static class PassagemStatus
    {
        public const string Confirmada = "Confirmada";
        public const string CheckIn = "Check-in";
        public const string Embarcada = "Embarcada";
        public const string Cancelada = "Cancelada";

        public static bool IsValid(string status)
        {
            return status == Confirmada || status == CheckIn || status == Embarcada || status == Cancelada;
        }

        public static string[] GetAll()
        {
            return new[] { Confirmada, CheckIn, Embarcada, Cancelada };
        }
    }
}