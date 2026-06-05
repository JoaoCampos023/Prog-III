namespace SistemaAereo.Models.ViewModels
{
    // ViewModel genérico para paginação de listas
    // T é o tipo da entidade sendo paginada
    public class PaginationViewModel<T>
    {
        // Lista de itens da página atual
        public IEnumerable<T> Items { get; set; }

        // Número da página atual
        public int CurrentPage { get; set; }

        // Total de páginas disponíveis
        public int TotalPages { get; set; }

        // Quantidade de itens por página
        public int ItemsPerPage { get; set; }

        // Total de itens no banco de dados
        public int TotalItems { get; set; }

        // Indica se existe página anterior
        public bool HasPreviousPage => CurrentPage > 1;

        // Indica se existe próxima página
        public bool HasNextPage => CurrentPage < TotalPages;

        // Construtor padrão (inicializa lista vazia)
        public PaginationViewModel()
        {
            Items = new List<T>();
        }

        // Construtor com parâmetros
        public PaginationViewModel(IEnumerable<T> items, int totalItems, int currentPage, int itemsPerPage)
        {
            Items = items;
            TotalItems = totalItems;
            CurrentPage = currentPage;
            ItemsPerPage = itemsPerPage;
            TotalPages = (int)Math.Ceiling(totalItems / (double)itemsPerPage);
        }
    }
}