namespace SistemaAereo.Models.Enums
{
    // Constantes para as classes de poltrona
    public static class SeatClass
    {
        // Classe econômica (mais barata)
        public const string Economy = "Economica";

        // Classe executiva (intermediária)
        public const string Executive = "Executiva";

        // Primeira classe (mais cara)
        public const string FirstClass = "Primeira";

        // Verifica se uma classe é válida
        public static bool IsValid(string seatClass)
        {
            return seatClass == Economy || seatClass == Executive || seatClass == FirstClass;
        }
    }
}