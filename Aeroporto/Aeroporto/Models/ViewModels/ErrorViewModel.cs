namespace SistemaAereo.Models.ViewModels
{
    // ViewModel para página de erro
    public class ErrorViewModel
    {
        // ID da requisição que gerou o erro
        public string? RequestId { get; set; }

        // Indica se deve mostrar o ID da requisição
        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
    }
}