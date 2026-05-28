namespace SistemaAereo.Models.Enums
{
    public static class SeatLocation
    {
        public const string Window = "Janela";
        public const string Aisle = "Corredor";
        public const string Middle = "Meio";

        public static bool IsValid(string location)
        {
            return location == Window || location == Aisle || location == Middle;
        }
    }
}