using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaAereo.Data.Context;
using SistemaAereo.Models.ViewModels;
using SistemaAereo.Repositories.Interfaces;

namespace SistemaAereo.Controllers
{
    public class HomeController : Controller
    {
        private readonly AeroportoContext _context;
        private readonly ILogger<HomeController> _logger;
        private readonly IClientePreferencialRepository _clienteRepository;
        private readonly IVooRepository _vooRepository;

        public HomeController(
            AeroportoContext context,
            ILogger<HomeController> logger,
            IClientePreferencialRepository clienteRepository,
            IVooRepository vooRepository)
        {
            _context = context;
            _logger = logger;
            _clienteRepository = clienteRepository;
            _vooRepository = vooRepository;
        }

        // =============================================
        // MÉTODOS PRINCIPAIS
        // =============================================

        public async Task<IActionResult> Index()
        {
            var model = new DashboardViewModel();

            await CarregarDadosPrincipais(model);
            await CarregarDadosPassagens(model);
            await CarregarDadosFaturamento(model);
            await CarregarPassagensRecentes(model);

            return View(model);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View();
        }

        // =============================================
        // MÉTODOS PRIVADOS AUXILIARES
        // =============================================

        private async Task CarregarDadosPrincipais(DashboardViewModel model)
        {
            model.TotalVoos = await _context.Voos.CountAsync(v => v.HorarioSaida > DateTime.Now);
            model.TotalClientes = await _context.ClientesPreferenciais.CountAsync(c => c.Ativo);
            model.TotalAeronaves = await _context.Aeronaves.CountAsync();
            model.TotalAeroportos = await _context.Aeroportos.CountAsync();

            model.ProximosVoos = await _context.Voos
                .Include(v => v.AeroportoOrigem)
                .Include(v => v.AeroportoDestino)
                .Include(v => v.Aeronave)
                .Where(v => v.HorarioSaida > DateTime.Now)
                .OrderBy(v => v.HorarioSaida)
                .Take(5)
                .ToListAsync();
        }

        private async Task CarregarDadosPassagens(DashboardViewModel model)
        {
            model.TotalPassagens = await _context.Passagens.CountAsync();
            model.PassagensConfirmadas = await _context.Passagens.CountAsync(p => p.Status == "Confirmada");
            model.PassagensCheckin = await _context.Passagens.CountAsync(p => p.Status == "Check-in");
            model.PassagensEmbarcadas = await _context.Passagens.CountAsync(p => p.Status == "Embarcada");
            model.PassagensCanceladas = await _context.Passagens.CountAsync(p => p.Status == "Cancelada");
        }

        private async Task CarregarDadosFaturamento(DashboardViewModel model)
        {
            model.FaturamentoTotal = await _context.Passagens
                .Where(p => p.Status != "Cancelada")
                .SumAsync(p => p.Preco);

            var inicioMes = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            var fimMes = inicioMes.AddMonths(1).AddDays(-1);

            model.FaturamentoMesAtual = await _context.Passagens
                .Where(p => p.DataEmissao >= inicioMes && p.DataEmissao <= fimMes && p.Status != "Cancelada")
                .SumAsync(p => p.Preco);
        }

        private async Task CarregarPassagensRecentes(DashboardViewModel model)
        {
            model.PassagensRecentes = await _context.Passagens
                .Include(p => p.Cliente)
                .Include(p => p.Voo)
                .OrderByDescending(p => p.DataEmissao)
                .Take(5)
                .ToListAsync();
        }
    }
}