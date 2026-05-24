namespace SistemaAereo.Models.Enums
{
    public static class PoltronaTipo
    {
        public const string Economica = "Economica";
        public const string Executiva = "Executiva";
        public const string Primeira = "Primeira";

        public static bool IsValid(string tipo)
        {
            return tipo == Economica || tipo == Executiva || tipo == Primeira;
        }
    }
}