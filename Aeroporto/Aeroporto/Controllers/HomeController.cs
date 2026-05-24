using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaAereo.Data.Context;
using SistemaAereo.Models.Entities;
using SistemaAereo.Models.Enums;
using SistemaAereo.Models.ViewModels;

namespace SistemaAereo.Controllers
{
    [Authorize] // Adicionar esta linha para exigir login
    public class HomeController : Controller
    {
        private readonly AeroportoContext _context;
        private readonly ILogger<HomeController> _logger;

        public HomeController(
            AeroportoContext context,
            ILogger<HomeController> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            var model = new DashboardViewModel();

            model.TotalVoos = await _context.Voos.CountAsync(v => v.HorarioSaida > DateTime.Now);
            model.TotalClientes = await _context.ClientesPreferenciais.CountAsync(c => c.Ativo);
            model.TotalAeronaves = await _context.Aeronaves.CountAsync();
            model.TotalAeroportos = await _context.Aeroportos.CountAsync();

            model.TotalPassagens = await _context.Passagens.CountAsync();
            model.PassagensConfirmadas = await _context.Passagens.CountAsync(p => p.Status == PassagemStatus.Confirmada);
            model.PassagensCheckin = await _context.Passagens.CountAsync(p => p.Status == PassagemStatus.CheckIn);
            model.PassagensEmbarcadas = await _context.Passagens.CountAsync(p => p.Status == PassagemStatus.Embarcada);
            model.PassagensCanceladas = await _context.Passagens.CountAsync(p => p.Status == PassagemStatus.Cancelada);

            model.FaturamentoTotal = await _context.Passagens
                .Where(p => p.Status != PassagemStatus.Cancelada)
                .SumAsync(p => p.Preco);

            var inicioMes = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            var fimMes = inicioMes.AddMonths(1).AddDays(-1);

            model.FaturamentoMesAtual = await _context.Passagens
                .Where(p => p.DataEmissao >= inicioMes && p.DataEmissao <= fimMes && p.Status != PassagemStatus.Cancelada)
                .SumAsync(p => p.Preco);

            model.ProximosVoos = await _context.Voos
                .Include(v => v.AeroportoOrigem)
                .Include(v => v.AeroportoDestino)
                .Include(v => v.Aeronave)
                .Where(v => v.HorarioSaida > DateTime.Now)
                .OrderBy(v => v.HorarioSaida)
                .Take(5)
                .ToListAsync();

            model.PassagensRecentes = await _context.Passagens
                .Include(p => p.Cliente)
                .Include(p => p.Voo)
                .OrderByDescending(p => p.DataEmissao)
                .Take(5)
                .ToListAsync();

            return View(model);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}