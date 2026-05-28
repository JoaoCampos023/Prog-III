namespace SistemaAereo.Models.Enums
{
    public static class FlightStatus
    {
        public const string Upcoming = "Futuro";
        public const string Today = "Hoje";
        public const string Past = "Passado";
        public const string Cancelled = "Cancelado";

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