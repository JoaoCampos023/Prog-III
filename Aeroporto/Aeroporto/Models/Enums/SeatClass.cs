namespace SistemaAereo.Models.Enums
{
    public static class SeatClass
    {
        public const string Economy = "Economica";
        public const string Executive = "Executiva";
        public const string FirstClass = "Primeira";

        public static bool IsValid(string seatClass)
        {
            return seatClass == Economy || seatClass == Executive || seatClass == FirstClass;
        }
    }
}