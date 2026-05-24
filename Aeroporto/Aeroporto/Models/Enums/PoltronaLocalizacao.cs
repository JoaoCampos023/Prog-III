namespace SistemaAereo.Models.Enums
{
    public static class PoltronaLocalizacao
    {
        public const string Janela = "Janela";
        public const string Corredor = "Corredor";
        public const string Meio = "Meio";

        public static bool IsValid(string localizacao)
        {
            return localizacao == Janela || localizacao == Corredor || localizacao == Meio;
        }
    }
}