namespace SistemaAereo.Models.ViewModels
{
    public class PaginationViewModel<T>
    {
        public IEnumerable<T> Items { get; set; }
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
        public int ItemsPerPage { get; set; }
        public int TotalItems { get; set; }

        public bool HasPreviousPage => CurrentPage > 1;
        public bool HasNextPage => CurrentPage < TotalPages;

        public PaginationViewModel()
        {
            Items = new List<T>();
        }

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