using System.ComponentModel.DataAnnotations;

namespace SistemaAereo.Models
{
    public class DashboardViewModel
    {
        // =============================================
        // ESTATÍSTICAS PRINCIPAIS
        // =============================================

        public int TotalVoos { get; set; }
        public int TotalClientes { get; set; }
        public int TotalAeronaves { get; set; }
        public int TotalAeroportos { get; set; }

        // =============================================
        // ESTATÍSTICAS DE PASSAGENS
        // =============================================

        public int TotalPassagens { get; set; }
        public int PassagensConfirmadas { get; set; }
        public int PassagensCheckin { get; set; }
        public int PassagensEmbarcadas { get; set; }
        public int PassagensCanceladas { get; set; }

        // =============================================
        // DADOS FINANCEIROS
        // =============================================

        public decimal FaturamentoTotal { get; set; }
        public decimal FaturamentoMesAtual { get; set; }

        // =============================================
        // LISTAS DE DADOS
        // =============================================

        public List<Voo> ProximosVoos { get; set; } = new List<Voo>();
        public List<Passagem> PassagensRecentes { get; set; } = new List<Passagem>();
    }
}