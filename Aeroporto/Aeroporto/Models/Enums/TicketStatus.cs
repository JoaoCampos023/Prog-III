namespace SistemaAereo.Models.Enums
{
    // Constantes para os possíveis status de uma passagem
    public static class TicketStatus
    {
        // Passagem confirmada (recém emitida)
        public const string Confirmed = "Confirmada";

        // Check-in já realizado
        public const string CheckIn = "Check-in";

        // Passageiro já embarcou
        public const string Boarded = "Embarcada";

        // Passagem cancelada
        public const string Cancelled = "Cancelada";

        // Verifica se um status é válido
        public static bool IsValid(string status)
        {
            return status == Confirmed || status == CheckIn || status == Boarded || status == Cancelled;
        }

        // Retorna todos os status disponíveis
        public static string[] GetAll()
        {
            return new[] { Confirmed, CheckIn, Boarded, Cancelled };
        }
    }
}