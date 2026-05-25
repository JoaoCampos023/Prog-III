using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SistemaAereo.Data.Context;
using SistemaAereo.Facades.Interfaces;
using SistemaAereo.Models.DTOs;
using SistemaAereo.Models.Entities;
using SistemaAereo.Models.Enums;
using SistemaAereo.Models.ViewModels;

namespace SistemaAereo.Controllers
{
    public class PassagensController : Controller
    {
        private readonly IPassagemFacade _passagemFacade;
        private readonly AeroportoContext _context;
        private readonly ILogger<PassagensController> _logger;

        public PassagensController(
            IPassagemFacade passagemFacade,
            AeroportoContext context,
            ILogger<PassagensController> logger)
        {
            _passagemFacade = passagemFacade;
            _context = context;
            _logger = logger;
        }

        // GET: Passagens
        public async Task<IActionResult> Index(int pagina = 1, int itensPorPagina = 10, string status = null)
        {
            try
            {
                var query = _context.Passagens
                    .AsNoTracking()
                    .Include(p => p.Voo)
                        .ThenInclude(v => v.AeroportoOrigem)
                    .Include(p => p.Voo)
                        .ThenInclude(v => v.AeroportoDestino)
                    .Include(p => p.Cliente)
                    .Include(p => p.Poltrona)
                    .AsQueryable();

                if (!string.IsNullOrEmpty(status) && PassagemStatus.IsValid(status))
                {
                    query = query.Where(p => p.Status == status);
                    ViewBag.StatusFiltro = status;
                }

                var totalItens = await query.CountAsync();
                var passagens = await query
                    .OrderByDescending(p => p.DataEmissao)
                    .Skip((pagina - 1) * itensPorPagina)
                    .Take(itensPorPagina)
                    .ToListAsync();

                var model = new PaginacaoViewModel<Passagem>(passagens, totalItens, pagina, itensPorPagina);

                ViewBag.ItensPorPaginaOptions = new[] { 5, 10, 25, 50, 100 };
                ViewBag.ItensPorPaginaAtual = itensPorPagina;
                ViewBag.StatusOptions = PassagemStatus.GetAll();

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao carregar passagens");
                TempData["Erro"] = "Erro ao carregar lista de passagens";
                return View(new PaginacaoViewModel<Passagem>());
            }
        }

        // GET: Passagens/Details/5
        public async Task<IActionResult> Details(int id)
        {
            try
            {
                var passagem = await _passagemFacade.ObterPassagemCompletaAsync(id);
                if (passagem == null)
                {
                    TempData["Erro"] = "Passagem não encontrada";
                    return RedirectToAction(nameof(Index));
                }
                return View(passagem);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao carregar detalhes da passagem {PassagemId}", id);
                TempData["Erro"] = "Erro ao carregar passagem";
                return RedirectToAction(nameof(Index));
            }
        }

        // GET: Passagens/Create
        public async Task<IActionResult> Create()
        {
            await CarregarViewBags();
            return View();
        }

        // POST: Passagens/Create - USANDO FACADE
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(EmitirPassagemRequestDto request)
        {
            try
            {
                _logger.LogInformation("=== TENTATIVA DE CRIAÇÃO DE PASSAGEM VIA FACADE ===");

                if (!ModelState.IsValid)
                {
                    await CarregarViewBags();
                    return View(request);
                }

                var result = await _passagemFacade.EmitirPassagemAsync(request);

                if (result.Success)
                {
                    TempData["Sucesso"] = $"Passagem emitida com sucesso! Número: {result.NumeroBilhete}";
                    return RedirectToAction(nameof(Details), new { id = result.PassagemId });
                }

                ModelState.AddModelError("", result.ErrorMessage);
                await CarregarViewBags();
                return View(request);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao criar passagem");
                TempData["Erro"] = "Erro ao criar passagem";
                await CarregarViewBags();
                return View(request);
            }
        }

        // GET: Passagens/Checkin/5
        public async Task<IActionResult> Checkin(int id)
        {
            var result = await _passagemFacade.RealizarCheckinAsync(new CheckinRequestDto { PassagemId = id });

            if (result.Success)
                TempData["Sucesso"] = result.Message;
            else
                TempData["Erro"] = result.ErrorMessage;

            return RedirectToAction(nameof(Details), new { id = id });
        }

        // GET: Passagens/Embarque/5
        public async Task<IActionResult> Embarque(int id)
        {
            var result = await _passagemFacade.RegistrarEmbarqueAsync(id);

            if (result.Success)
                TempData["Sucesso"] = result.Message;
            else
                TempData["Erro"] = result.ErrorMessage;

            return RedirectToAction(nameof(Details), new { id = id });
        }

        // GET: Passagens/Cancelar/5
        public async Task<IActionResult> Cancelar(int id)
        {
            var result = await _passagemFacade.CancelarPassagemAsync(new CancelarPassagemRequestDto { PassagemId = id });

            if (result.Success)
                TempData["Sucesso"] = result.Message;
            else
                TempData["Erro"] = result.ErrorMessage;

            return RedirectToAction(nameof(Index));
        }

        // GET: Passagens/PorCliente/5
        public async Task<IActionResult> PorCliente(int id, int pagina = 1, int itensPorPagina = 10)
        {
            try
            {
                var cliente = await _context.ClientesPreferenciais.FindAsync(id);
                if (cliente == null)
                {
                    TempData["Erro"] = "Cliente não encontrado";
                    return RedirectToAction(nameof(Index));
                }

                ViewBag.ClienteNome = cliente.Nome;
                ViewBag.ClienteId = id;

                var query = _context.Passagens
                    .AsNoTracking()
                    .Include(p => p.Voo)
                        .ThenInclude(v => v.AeroportoOrigem)
                    .Include(p => p.Voo)
                        .ThenInclude(v => v.AeroportoDestino)
                    .Include(p => p.Poltrona)
                    .Where(p => p.ClienteId == id);

                var totalItens = await query.CountAsync();
                var passagens = await query
                    .OrderByDescending(p => p.DataEmissao)
                    .Skip((pagina - 1) * itensPorPagina)
                    .Take(itensPorPagina)
                    .ToListAsync();

                var model = new PaginacaoViewModel<Passagem>(passagens, totalItens, pagina, itensPorPagina);
                ViewBag.ItensPorPaginaOptions = new[] { 5, 10, 25, 50, 100 };
                ViewBag.ItensPorPaginaAtual = itensPorPagina;

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao carregar passagens do cliente {ClienteId}", id);
                TempData["Erro"] = "Erro ao carregar passagens do cliente";
                return View(new PaginacaoViewModel<Passagem>());
            }
        }

        // GET: Passagens/PorVoo/5
        public async Task<IActionResult> PorVoo(int id, int pagina = 1, int itensPorPagina = 10)
        {
            try
            {
                var voo = await _context.Voos.FindAsync(id);
                if (voo == null)
                {
                    TempData["Erro"] = "Voo não encontrado";
                    return RedirectToAction(nameof(Index));
                }

                ViewBag.VooNumero = voo.NumeroVoo;
                ViewBag.VooId = id;

                var query = _context.Passagens
                    .AsNoTracking()
                    .Include(p => p.Cliente)
                    .Include(p => p.Poltrona)
                    .Where(p => p.VooId == id);

                var totalItens = await query.CountAsync();
                var passagens = await query
                    .OrderBy(p => p.Poltrona.NumeroPoltrona)
                    .Skip((pagina - 1) * itensPorPagina)
                    .Take(itensPorPagina)
                    .ToListAsync();

                var model = new PaginacaoViewModel<Passagem>(passagens, totalItens, pagina, itensPorPagina);
                ViewBag.ItensPorPaginaOptions = new[] { 5, 10, 25, 50, 100 };
                ViewBag.ItensPorPaginaAtual = itensPorPagina;

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao carregar passagens do voo {VooId}", id);
                TempData["Erro"] = "Erro ao carregar passagens do voo";
                return View(new PaginacaoViewModel<Passagem>());
            }
        }

        // GET: Passagens/PoltronasDisponiveis
        public async Task<JsonResult> PoltronasDisponiveis(int vooId)
        {
            try
            {
                var poltronas = await _context.Poltronas
                    .AsNoTracking()
                    .Where(p => p.VooId == vooId && p.Disponivel)
                    .OrderBy(p => p.NumeroPoltrona)
                    .ToListAsync();

                var result = poltronas.Select(p => new
                {
                    poltronaId = p.PoltronaId,
                    numeroPoltrona = p.NumeroPoltrona,
                    tipo = p.Tipo,
                    localizacao = p.Localizacao,
                    preco = p.Preco
                }).ToList();

                return Json(new { success = true, data = result });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao carregar poltronas para voo {VooId}", vooId);
                return Json(new { success = false, message = "Erro ao carregar poltronas" });
            }
        }

        // =============================================
        // MÉTODOS PRIVADOS AUXILIARES
        // =============================================

        private async Task CarregarViewBags()
        {
            var clientesAtivos = await _context.ClientesPreferenciais
                .Where(c => c.Ativo)
                .OrderBy(c => c.Nome)
                .ToListAsync();

            ViewBag.Clientes = new SelectList(clientesAtivos, "ClienteId", "Nome");

            var agora = DateTime.Now;
            var voosDisponiveis = await _context.Voos
                .Include(v => v.AeroportoOrigem)
                .Include(v => v.AeroportoDestino)
                .Include(v => v.Poltronas)
                .Where(v => v.HorarioSaida > agora && v.Poltronas.Any(p => p.Disponivel))
                .OrderBy(v => v.HorarioSaida)
                .ToListAsync();

            if (voosDisponiveis.Any())
            {
                var voosSelectList = voosDisponiveis.Select(v => new
                {
                    VooId = v.VooId,
                    DisplayText = $"{v.NumeroVoo} - {v.AeroportoOrigem?.CodigoIATA ?? "N/A"} → {v.AeroportoDestino?.CodigoIATA ?? "N/A"} - {v.HorarioSaida:dd/MM/yyyy HH:mm}"
                }).ToList();

                ViewBag.VoosDetalhados = new SelectList(voosSelectList, "VooId", "DisplayText");
            }
            else
            {
                ViewBag.VoosDetalhados = new SelectList(new List<object>(), "VooId", "DisplayText");
            }
        }
    }
}