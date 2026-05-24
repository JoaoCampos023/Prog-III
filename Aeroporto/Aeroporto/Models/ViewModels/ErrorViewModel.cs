namespace SistemaAereo.Models.ViewModels
{
    public class ErrorViewModel
    {
        // =============================================
        // PROPRIEDADES DE INFORMAÇÃO DE ERRO
        // =============================================

        public string? RequestId { get; set; }

        // =============================================
        // PROPRIEDADE CALCULADA
        // =============================================

        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
    }
}