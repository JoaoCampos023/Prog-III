namespace SistemaAereo.Models.Enums
{
    public static class TicketStatus
    {
        public const string Confirmed = "Confirmada";
        public const string CheckIn = "Check-in";
        public const string Boarded = "Embarcada";
        public const string Cancelled = "Cancelada";

        public static bool IsValid(string status)
        {
            return status == Confirmed || status == CheckIn || status == Boarded || status == Cancelled;
        }

        public static string[] GetAll()
        {
            return new[] { Confirmed, CheckIn, Boarded, Cancelled };
        }
    }
}