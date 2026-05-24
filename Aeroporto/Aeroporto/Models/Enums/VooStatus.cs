namespace SistemaAereo.Models.Enums
{
    public static class VooStatus
    {
        public const string Futuro = "Futuro";
        public const string Hoje = "Hoje";
        public const string Passado = "Passado";
        public const string Cancelado = "Cancelado";

        public static string GetStatus(DateTime horarioSaida)
        {
            if (horarioSaida.Date == DateTime.Today)
                return Hoje;
            if (horarioSaida > DateTime.Now)
                return Futuro;
            return Passado;
        }
    }
}