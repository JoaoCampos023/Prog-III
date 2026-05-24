namespace SistemaAereo.Models.ViewModels
{
    public class PaginacaoViewModel<T>
    {
        public IEnumerable<T> Itens { get; set; }
        public int PaginaAtual { get; set; }
        public int TotalPaginas { get; set; }
        public int ItensPorPagina { get; set; }
        public int TotalItens { get; set; }

        public bool TemPaginaAnterior => PaginaAtual > 1;
        public bool TemProximaPagina => PaginaAtual < TotalPaginas;

        public PaginacaoViewModel()
        {
            Itens = new List<T>();
        }

        public PaginacaoViewModel(IEnumerable<T> itens, int totalItens, int paginaAtual, int itensPorPagina)
        {
            Itens = itens;
            TotalItens = totalItens;
            PaginaAtual = paginaAtual;
            ItensPorPagina = itensPorPagina;
            TotalPaginas = (int)Math.Ceiling(totalItens / (double)itensPorPagina);
        }
    }
}