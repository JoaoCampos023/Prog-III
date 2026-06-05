namespace SistemaAereo.Models.Enums
{
    // Constantes para localização da poltrona na fileira
    public static class SeatLocation
    {
        // Poltrona próxima à janela
        public const string Window = "Janela";

        // Poltrona próxima ao corredor
        public const string Aisle = "Corredor";

        // Poltrona no meio da fileira
        public const string Middle = "Meio";

        // Verifica se uma localização é válida
        public static bool IsValid(string location)
        {
            return location == Window || location == Aisle || location == Middle;
        }
    }
}