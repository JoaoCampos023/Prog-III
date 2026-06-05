namespace SistemaAereo.Models.Enums
{
    // Constantes para classificação de status de voo
    public static class FlightStatus
    {
        // Voo futuro (data de saída > hoje)
        public const string Upcoming = "Futuro";

        // Voo de hoje (data de saída = hoje)
        public const string Today = "Hoje";

        // Voo passado (data de saída < hoje)
        public const string Past = "Passado";

        // Voo cancelado
        public const string Cancelled = "Cancelado";

        // Obtém o status baseado na data de saída
        public static string GetStatus(DateTime departureTime)
        {
            if (departureTime.Date == DateTime.Today)
                return Today;
            if (departureTime > DateTime.Now)
                return Upcoming;
            return Past;
        }
    }
}