using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SistemaAereo.Data.Context;
using SistemaAereo.Models.Entities;
using SistemaAereo.Models.Enums;
using SistemaAereo.Models.ViewModels;
using SistemaAereo.Repositories;
using SistemaAereo.Repositories.Interfaces;
using SistemaAereo.Services.Interfaces;

namespace SistemaAereo.Controllers
{
    public class PassagensController : Controller
    {
        private readonly IPassagemRepository _passagemRepository;
        private readonly IPoltronaRepository _poltronaRepository;
        private readonly IClientePreferencialRepository _clienteRepository;
        private readonly IVooRepository _vooRepository;
        private readonly IPoltronaService _poltronaService;
        private readonly AeroportoContext _context;
        private readonly ILogger<PassagensController> _logger;

        public PassagensController(
            IPassagemRepository passagemRepository,
            IPoltronaRepository poltronaRepository,
            IClientePreferencialRepository clienteRepository,
            IVooRepository vooRepository,
            IPoltronaService poltronaService,
            AeroportoContext context,
            ILogger<PassagensController> logger)
        {
            _passagemRepository = passagemRepository;
            _poltronaRepository = poltronaRepository;
            _clienteRepository = clienteRepository;
            _vooRepository = vooRepository;
            _poltronaService = poltronaService;
            _context = context;
            _logger = logger;
        }

        // =============================================
        // MÉTODOS PRINCIPAIS - CRUD
        // =============================================

        // GET: Passagens (COM PAGINAÇÃO)
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

                // Filtro por status
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

                // Opções de paginação para a view
                ViewBag.ItensPorPaginaOptions = new[] { 5, 10, 25, 50, 100 };
                ViewBag.ItensPorPaginaAtual = itensPorPagina;
                ViewBag.StatusOptions = PassagemStatus.GetAll();

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao carregar passagens");
                TempData["Erro"] = "Erro ao carregar lista de passagens";

                // Garantir que os ViewBags sejam definidos mesmo em caso de erro
                ViewBag.ItensPorPaginaOptions = new[] { 5, 10, 25, 50, 100 };
                ViewBag.ItensPorPaginaAtual = itensPorPagina;
                ViewBag.StatusOptions = PassagemStatus.GetAll();

                return View(new PaginacaoViewModel<Passagem>());
            }
        }

        // GET: Passagens/Details/5
        public async Task<IActionResult> Details(int id)
        {
            try
            {
                var passagem = await _context.Passagens
                    .AsNoTracking()
                    .Include(p => p.Voo)
                        .ThenInclude(v => v.AeroportoOrigem)
                    .Include(p => p.Voo)
                        .ThenInclude(v => v.AeroportoDestino)
                    .Include(p => p.Voo)
                        .ThenInclude(v => v.Aeronave)
                    .Include(p => p.Cliente)
                    .Include(p => p.Poltrona)
                    .FirstOrDefaultAsync(p => p.PassagemId == id);

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

        // =============================================
        // CRIAÇÃO DE PASSAGENS
        // =============================================

        // GET: Passagens/Create
        public async Task<IActionResult> Create()
        {
            try
            {
                _logger.LogInformation("=== ACESSANDO Passagens/Create GET ===");
                await CarregarViewBags();
                _logger.LogInformation("Página Create carregada com sucesso");
                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ERRO ao carregar página Create");
                TempData["Erro"] = "Erro ao carregar formulário de passagem";
                return View();
            }
        }

        // POST: Passagens/Create (COM TRANSAÇÃO E CONTROLE DE CONCORRÊNCIA)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Passagem passagem)
        {
            try
            {
                _logger.LogInformation("=== TENTATIVA DE CRIAÇÃO DE PASSAGEM ===");

                ModelState.Clear();
                ValidarPassagem(passagem);

                if (!ModelState.IsValid)
                {
                    _logger.LogWarning("Validação manual falhou");
                    await CarregarViewBags();
                    return View(passagem);
                }

                _logger.LogInformation($"Dados válidos - Cliente: {passagem.ClienteId}, Voo: {passagem.VooId}, Poltrona: {passagem.PoltronaId}");

                // Usar transação para garantir consistência
                using var transaction = await _context.Database.BeginTransactionAsync();

                try
                {
                    // Recarregar poltrona com bloqueio para controle de concorrência
                    var poltrona = await _context.Poltronas
                        .FirstOrDefaultAsync(p => p.PoltronaId == passagem.PoltronaId && p.Disponivel);

                    if (poltrona == null)
                    {
                        ModelState.AddModelError("PoltronaId", "Poltrona não encontrada ou não está mais disponível.");
                        await CarregarViewBags();
                        return View(passagem);
                    }

                    var cliente = await ValidarCliente(passagem.ClienteId);
                    if (cliente == null)
                    {
                        await CarregarViewBags();
                        return View(passagem);
                    }

                    var voo = await ValidarVoo(passagem.VooId);
                    if (voo == null)
                    {
                        await CarregarViewBags();
                        return View(passagem);
                    }

                    // Verificar dupla ocupação
                    if (await PoltronaOcupada(passagem.PoltronaId))
                    {
                        ModelState.AddModelError("PoltronaId", "Poltrona já ocupada.");
                        await CarregarViewBags();
                        return View(passagem);
                    }

                    // Verificar se o voo já partiu
                    if (voo.HorarioSaida < DateTime.Now)
                    {
                        ModelState.AddModelError("VooId", "Não é possível comprar passagem para um voo que já partiu.");
                        await CarregarViewBags();
                        return View(passagem);
                    }

                    // Marcar poltrona como indisponível primeiro
                    poltrona.Disponivel = false;
                    _context.Poltronas.Update(poltrona);
                    await _context.SaveChangesAsync();

                    // Preencher e salvar passagem
                    PreencherDadosPassagem(passagem, poltrona);
                    _context.Passagens.Add(passagem);
                    await _context.SaveChangesAsync();

                    await transaction.CommitAsync();

                    TempData["Sucesso"] = $"Passagem emitida com sucesso! Número: {passagem.NumeroBilhete}";
                    return RedirectToAction(nameof(Details), new { id = passagem.PassagemId });
                }
                catch (DbUpdateConcurrencyException ex)
                {
                    await transaction.RollbackAsync();
                    _logger.LogError(ex, "Conflito de concorrência ao criar passagem");
                    ModelState.AddModelError("", "A poltrona foi comprada por outro usuário. Tente novamente.");
                    await CarregarViewBags();
                    return View(passagem);
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    _logger.LogError(ex, "Erro ao criar passagem");
                    ModelState.AddModelError("", "Erro ao processar a compra. Tente novamente.");
                    await CarregarViewBags();
                    return View(passagem);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao criar passagem");
                TempData["Erro"] = "Erro ao criar passagem: " + ex.Message;
                await CarregarViewBags();
                return View(passagem);
            }
        }

        // =============================================
        // OPERAÇÕES DE PASSAGEM
        // =============================================

        // GET: Passagens/Checkin/5
        public async Task<IActionResult> Checkin(int id)
        {
            try
            {
                var passagem = await _passagemRepository.GetByIdAsync(id);
                if (passagem == null)
                {
                    TempData["Erro"] = "Passagem não encontrada";
                    return RedirectToAction(nameof(Index));
                }

                if (passagem.Status != PassagemStatus.Confirmada)
                {
                    TempData["Erro"] = $"Check-in não permitido. Status atual: {passagem.Status}";
                    return RedirectToAction(nameof(Details), new { id = id });
                }

                // Verificar se o voo já partiu
                var voo = await _vooRepository.GetByIdAsync(passagem.VooId);
                if (voo != null && voo.HorarioSaida < DateTime.Now)
                {
                    TempData["Erro"] = "Não é possível fazer check-in de um voo que já partiu.";
                    return RedirectToAction(nameof(Details), new { id = id });
                }

                passagem.Status = PassagemStatus.CheckIn;
                await _passagemRepository.UpdateAsync(passagem);

                TempData["Sucesso"] = "Check-in realizado com sucesso!";
                return RedirectToAction(nameof(Details), new { id = id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao realizar check-in da passagem {PassagemId}", id);
                TempData["Erro"] = "Erro ao realizar check-in";
                return RedirectToAction(nameof(Index));
            }
        }

        // GET: Passagens/Embarque/5
        public async Task<IActionResult> Embarque(int id)
        {
            try
            {
                var passagem = await _passagemRepository.GetByIdAsync(id);
                if (passagem == null)
                {
                    TempData["Erro"] = "Passagem não encontrada";
                    return RedirectToAction(nameof(Index));
                }

                if (passagem.Status != PassagemStatus.CheckIn)
                {
                    TempData["Erro"] = $"Embarque não permitido. Status atual: {passagem.Status}. É necessário fazer check-in primeiro.";
                    return RedirectToAction(nameof(Details), new { id = id });
                }

                passagem.Status = PassagemStatus.Embarcada;
                await _passagemRepository.UpdateAsync(passagem);

                TempData["Sucesso"] = "Embarque registrado com sucesso!";
                return RedirectToAction(nameof(Details), new { id = id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao registrar embarque da passagem {PassagemId}", id);
                TempData["Erro"] = "Erro ao registrar embarque";
                return RedirectToAction(nameof(Index));
            }
        }

        // GET: Passagens/Cancelar/5 (COM TRANSAÇÃO)
        public async Task<IActionResult> Cancelar(int id)
        {
            try
            {
                var passagem = await _context.Passagens
                    .Include(p => p.Voo)
                    .FirstOrDefaultAsync(p => p.PassagemId == id);

                if (passagem == null)
                {
                    TempData["Erro"] = "Passagem não encontrada";
                    return RedirectToAction(nameof(Index));
                }

                if (passagem.Status == PassagemStatus.Cancelada)
                {
                    TempData["Info"] = "Esta passagem já está cancelada.";
                    return RedirectToAction(nameof(Details), new { id = id });
                }

                // Verificar se o voo já partiu
                if (passagem.Voo != null && passagem.Voo.HorarioSaida < DateTime.Now)
                {
                    TempData["Erro"] = "Não é possível cancelar uma passagem de um voo que já partiu.";
                    return RedirectToAction(nameof(Details), new { id = id });
                }

                // Usar transação
                using var transaction = await _context.Database.BeginTransactionAsync();

                try
                {
                    passagem.Status = PassagemStatus.Cancelada;
                    _context.Passagens.Update(passagem);
                    await _context.SaveChangesAsync();

                    var poltrona = await _poltronaRepository.GetByIdAsync(passagem.PoltronaId);
                    if (poltrona != null)
                    {
                        poltrona.Disponivel = true;
                        _context.Poltronas.Update(poltrona);
                        await _context.SaveChangesAsync();
                    }

                    await transaction.CommitAsync();

                    TempData["Sucesso"] = "Passagem cancelada com sucesso! A poltrona foi liberada.";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    _logger.LogError(ex, "Erro ao cancelar passagem {PassagemId}", id);
                    TempData["Erro"] = "Erro ao cancelar passagem";
                    return RedirectToAction(nameof(Index));
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao cancelar passagem {PassagemId}", id);
                TempData["Erro"] = "Erro ao cancelar passagem";
                return RedirectToAction(nameof(Index));
            }
        }

        // =============================================
        // CONSULTAS ESPECÍFICAS
        // =============================================

        // GET: Passagens/PorCliente/5
        public async Task<IActionResult> PorCliente(int id, int pagina = 1, int itensPorPagina = 10)
        {
            try
            {
                var cliente = await _clienteRepository.GetByIdAsync(id);
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
                    .Where(p => p.ClienteId == id)
                    .AsQueryable();

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
                var voo = await _vooRepository.GetByIdAsync(id);
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
                    .Where(p => p.VooId == id)
                    .AsQueryable();

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

        // =============================================
        // MÉTODOS AJAX/JSON
        // =============================================

        // GET: Passagens/PoltronasDisponiveis
        public async Task<JsonResult> PoltronasDisponiveis(int vooId)
        {
            try
            {
                _logger.LogInformation($"Buscando poltronas para voo {vooId}");

                if (vooId <= 0)
                {
                    return Json(new { success = false, message = "ID do voo inválido" });
                }

                var poltronas = await _context.Poltronas
                    .AsNoTracking()
                    .Where(p => p.VooId == vooId && p.Disponivel)
                    .OrderBy(p => p.NumeroPoltrona)
                    .ToListAsync();

                _logger.LogInformation($"Encontradas {poltronas.Count} poltronas disponíveis para voo {vooId}");

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

        // GET: Passagens/BuscarDadosVoo
        public async Task<JsonResult> BuscarDadosVoo(int vooId)
        {
            try
            {
                var voo = await _context.Voos
                    .AsNoTracking()
                    .Include(v => v.AeroportoOrigem)
                    .Include(v => v.AeroportoDestino)
                    .Include(v => v.Aeronave)
                    .FirstOrDefaultAsync(v => v.VooId == vooId);

                if (voo == null)
                {
                    return Json(new { success = false, message = "Voo não encontrado" });
                }

                return Json(new
                {
                    success = true,
                    data = new
                    {
                        vooId = voo.VooId,
                        numeroVoo = voo.NumeroVoo,
                        origem = voo.AeroportoOrigem?.CodigoIATA,
                        destino = voo.AeroportoDestino?.CodigoIATA,
                        saida = voo.HorarioSaida.ToString("dd/MM/yyyy HH:mm"),
                        aeronave = voo.Aeronave?.TipoAeronave
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao buscar dados do voo {VooId}", vooId);
                return Json(new { success = false, message = "Erro ao buscar dados do voo" });
            }
        }

        // =============================================
        // MÉTODOS DE DIAGNÓSTICO
        // =============================================

        public async Task<IActionResult> VerificarDados()
        {
            try
            {
                var dados = new
                {
                    ClientesCount = await _context.ClientesPreferenciais.CountAsync(c => c.Ativo),
                    VoosCount = await _context.Voos.CountAsync(v => v.HorarioSaida > DateTime.Now),
                    VoosComPoltronasCount = await _context.Voos
                        .Include(v => v.Poltronas)
                        .CountAsync(v => v.HorarioSaida > DateTime.Now && v.Poltronas.Any(p => p.Disponivel)),
                    PoltronasDisponiveisCount = await _context.Poltronas.CountAsync(p => p.Disponivel)
                };

                return Json(new
                {
                    success = true,
                    message = "Dados verificados com sucesso",
                    data = dados
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // =============================================
        // MÉTODOS PRIVADOS AUXILIARES
        // =============================================

        private async Task CarregarViewBags()
        {
            try
            {
                _logger.LogInformation("=== INICIANDO CarregarViewBags ===");

                var clientesAtivos = await _context.ClientesPreferenciais
                    .AsNoTracking()
                    .Where(c => c.Ativo)
                    .OrderBy(c => c.Nome)
                    .ToListAsync();

                _logger.LogInformation($"Clientes ativos encontrados: {clientesAtivos.Count}");

                if (clientesAtivos.Any())
                {
                    ViewBag.Clientes = new SelectList(clientesAtivos, "ClienteId", "Nome");
                    _logger.LogInformation("ViewBag.Clientes carregado com sucesso");
                }
                else
                {
                    ViewBag.Clientes = new SelectList(new List<ClientePreferencial>(), "ClienteId", "Nome");
                    _logger.LogWarning("Nenhum cliente ativo encontrado");
                }

                var agora = DateTime.Now;
                var voosDisponiveis = await _context.Voos
                    .AsNoTracking()
                    .Include(v => v.AeroportoOrigem)
                    .Include(v => v.AeroportoDestino)
                    .Include(v => v.Poltronas)
                    .Where(v => v.HorarioSaida > agora && v.Poltronas.Any(p => p.Disponivel))
                    .OrderBy(v => v.HorarioSaida)
                    .ToListAsync();

                _logger.LogInformation($"Voos disponíveis encontrados: {voosDisponiveis.Count}");

                if (voosDisponiveis.Any())
                {
                    var voosSelectList = voosDisponiveis.Select(v => new
                    {
                        VooId = v.VooId,
                        DisplayText = $"{v.NumeroVoo} - {v.AeroportoOrigem?.CodigoIATA ?? "N/A"} → {v.AeroportoDestino?.CodigoIATA ?? "N/A"} - {v.HorarioSaida:dd/MM/yyyy HH:mm}"
                    }).ToList();

                    ViewBag.VoosDetalhados = new SelectList(voosSelectList, "VooId", "DisplayText");
                    _logger.LogInformation("ViewBag.VoosDetalhados carregado com sucesso");
                }
                else
                {
                    ViewBag.VoosDetalhados = new SelectList(new List<object>(), "VooId", "DisplayText");
                    _logger.LogWarning("Nenhum voo disponível encontrado");
                }

                _logger.LogInformation("=== CarregarViewBags FINALIZADO ===");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ERRO CRÍTICO em CarregarViewBags");
                ViewBag.Clientes = new SelectList(new List<ClientePreferencial>(), "ClienteId", "Nome");
                ViewBag.VoosDetalhados = new SelectList(new List<object>(), "VooId", "DisplayText");
            }
        }

        private string GerarNumeroBilhete()
        {
            // Usando GUID para garantir unicidade
            return Guid.NewGuid().ToString("N").Substring(0, 20).ToUpper();
        }

        private void ValidarPassagem(Passagem passagem)
        {
            if (passagem.ClienteId <= 0)
                ModelState.AddModelError("ClienteId", "Cliente é obrigatório.");

            if (passagem.VooId <= 0)
                ModelState.AddModelError("VooId", "Voo é obrigatório.");

            if (passagem.PoltronaId <= 0)
                ModelState.AddModelError("PoltronaId", "Poltrona é obrigatória.");
        }

        private async Task<ClientePreferencial> ValidarCliente(int clienteId)
        {
            var cliente = await _context.ClientesPreferenciais
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.ClienteId == clienteId && c.Ativo);

            if (cliente == null)
            {
                ModelState.AddModelError("ClienteId", "Cliente não encontrado ou inativo.");
                return null;
            }

            return cliente;
        }

        private async Task<Voo> ValidarVoo(int vooId)
        {
            var voo = await _context.Voos
                .AsNoTracking()
                .FirstOrDefaultAsync(v => v.VooId == vooId);

            if (voo == null)
            {
                ModelState.AddModelError("VooId", "Voo não encontrado.");
                return null;
            }

            return voo;
        }

        private async Task<bool> PoltronaOcupada(int poltronaId)
        {
            return await _context.Passagens
                .AsNoTracking()
                .AnyAsync(p => p.PoltronaId == poltronaId && p.Status != PassagemStatus.Cancelada);
        }

        private void PreencherDadosPassagem(Passagem passagem, Poltrona poltrona)
        {
            passagem.NumeroBilhete = GerarNumeroBilhete();
            passagem.DataEmissao = DateTime.Now;
            passagem.Status = PassagemStatus.Confirmada;
            passagem.Classe = poltrona.Tipo;
            passagem.Preco = poltrona.Preco;
        }
    }
}