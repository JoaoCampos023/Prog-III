using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SistemaAereo.Data;
using SistemaAereo.Models;

namespace SistemaAereo.Controllers
{
    public class VoosController : Controller
    {
        private readonly AeroportoContext _context;

        public VoosController(AeroportoContext context)
        {
            _context = context;
        }

        // =============================================
        // MÉTODOS PRINCIPAIS - CRUD
        // =============================================

        // GET: Voos
        public async Task<IActionResult> Index()
        {
            var voos = await _context.Voos
                .Include(v => v.AeroportoOrigem)
                .Include(v => v.AeroportoDestino)
                .Include(v => v.Aeronave)
                .OrderBy(v => v.HorarioSaida)
                .ToListAsync();

            return View(voos);
        }

        // GET: Voos/Create
        public async Task<IActionResult> Create()
        {
            await CarregarViewBags();
            return View(new Voo());
        }

        // POST: Voos/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
        string NumeroVoo,
        int AeroportoOrigemId,
        int AeroportoDestinoId,
        int AeronaveId,
        DateTime HorarioSaida,
        DateTime HorarioChegadaPrevisto)
        {
            try
            {
                Console.WriteLine("=== DADOS RECEBIDOS VIA PARÂMETROS ===");
                Console.WriteLine($"NumeroVoo: {NumeroVoo}");
                Console.WriteLine($"AeroportoOrigemId: {AeroportoOrigemId}");
                Console.WriteLine($"AeroportoDestinoId: {AeroportoDestinoId}");
                Console.WriteLine($"AeronaveId: {AeronaveId}");
                Console.WriteLine($"HorarioSaida: {HorarioSaida}");
                Console.WriteLine($"HorarioChegadaPrevisto: {HorarioChegadaPrevisto}");

                // Criar objeto Voo manualmente
                var voo = new Voo
                {
                    NumeroVoo = NumeroVoo?.Trim().ToUpper(),
                    AeroportoOrigemId = AeroportoOrigemId,
                    AeroportoDestinoId = AeroportoDestinoId,
                    AeronaveId = AeronaveId,
                    HorarioSaida = HorarioSaida,
                    HorarioChegadaPrevisto = HorarioChegadaPrevisto
                };

                // Validações manuais
                if (AeroportoOrigemId == AeroportoDestinoId)
                {
                    ModelState.AddModelError("AeroportoDestinoId", "O aeroporto de destino deve ser diferente do aeroporto de origem.");
                }

                if (HorarioChegadaPrevisto <= HorarioSaida)
                {
                    ModelState.AddModelError("HorarioChegadaPrevisto", "O horário de chegada deve ser posterior ao horário de saída.");
                }

                // CORREÇÃO: Usar await e evitar operações concorrentes
                var numeroVooExists = await _context.Voos
                    .AsNoTracking() // Importante: usar AsNoTracking para consultas
                    .AnyAsync(v => v.NumeroVoo == NumeroVoo);

                if (numeroVooExists)
                {
                    ModelState.AddModelError("NumeroVoo", "Este número de voo já está cadastrado.");
                }

                if (ModelState.IsValid)
                {
                    // CORREÇÃO: Usar apenas uma operação de SaveChanges por transação
                    _context.Voos.Add(voo);
                    await _context.SaveChangesAsync();

                    // CORREÇÃO: Criar poltronas em uma operação separada se necessário
                    await CriarPoltronasParaVoo(voo.VooId, voo.AeronaveId);

                    TempData["Sucesso"] = $"Voo {voo.NumeroVoo} cadastrado com sucesso!";
                    return RedirectToAction(nameof(Index));
                }

                await CarregarViewBags();
                return View(voo);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERRO: {ex.Message}");
                TempData["Erro"] = $"Erro: {ex.Message}";
                await CarregarViewBags();

                var voo = new Voo
                {
                    NumeroVoo = NumeroVoo,
                    AeroportoOrigemId = AeroportoOrigemId,
                    AeroportoDestinoId = AeroportoDestinoId,
                    AeronaveId = AeronaveId,
                    HorarioSaida = HorarioSaida,
                    HorarioChegadaPrevisto = HorarioChegadaPrevisto
                };

                return View(voo);
            }
        }

        private async Task CriarPoltronasParaVoo(int vooId, int aeronaveId)
        {
            try
            {
                // Buscar informações da aeronave
                var aeronave = await _context.Aeronaves
                    .AsNoTracking()
                    .FirstOrDefaultAsync(a => a.AeronaveId == aeronaveId);

                if (aeronave == null) return;

                var numeroPoltronas = aeronave.NumeroPoltronas;
                var poltronas = new List<Poltrona>();
                var random = new Random();

                for (int i = 1; i <= numeroPoltronas; i++)
                {
                    var fileira = (i - 1) / 6 + 1;
                    var assento = (i - 1) % 6 + 1;
                    var letraAssento = ((char)('A' + (assento - 1))).ToString();

                    var tipo = i <= numeroPoltronas * 0.2 ? "Executiva" : "Economica";
                    var localizacao = assento switch
                    {
                        1 or 6 => "Janela",
                        2 or 5 => "Meio",
                        3 or 4 => "Corredor",
                        _ => "Corredor"
                    };

                    var precoBase = tipo == "Executiva" ? 500.00m : 300.00m;
                    var preco = precoBase + (random.Next(-50, 51));

                    var poltrona = new Poltrona
                    {
                        VooId = vooId,
                        NumeroPoltrona = $"{fileira}{letraAssento}",
                        Disponivel = true,
                        Localizacao = localizacao,
                        Tipo = tipo,
                        Preco = preco
                    };

                    poltronas.Add(poltrona);
                }

                // CORREÇÃO: Usar uma única operação de SaveChanges
                await _context.Poltronas.AddRangeAsync(poltronas);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                // Log do erro, mas não interrompe o fluxo principal
                Console.WriteLine($"Erro ao criar poltronas: {ex.Message}");
            }
        }

        // GET: Voos/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var voo = await _context.Voos
                .Include(v => v.AeroportoOrigem)
                .Include(v => v.AeroportoDestino)
                .Include(v => v.Aeronave)
                .FirstOrDefaultAsync(v => v.VooId == id);

            if (voo == null) return RedirectToAction(nameof(Index));

            return View(voo);
        }

        // GET: Voos/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var voo = await _context.Voos.FindAsync(id);
            if (voo == null) return RedirectToAction(nameof(Index));

            await CarregarViewBags();
            return View(voo);
        }

        // POST: Voos/Edit/5
        [HttpPost]
        public async Task<IActionResult> Edit(int id, Voo voo)
        {
            if (id != voo.VooId) return RedirectToAction(nameof(Index));

            if (ModelState.IsValid)
            {
                _context.Update(voo);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            await CarregarViewBags();
            return View(voo);
        }

        // GET: Voos/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            var voo = await _context.Voos
                .Include(v => v.AeroportoOrigem)
                .Include(v => v.AeroportoDestino)
                .Include(v => v.Aeronave)
                .FirstOrDefaultAsync(v => v.VooId == id);

            if (voo == null) return RedirectToAction(nameof(Index));

            return View(voo);
        }

        // POST: Voos/Delete/5
        [HttpPost, ActionName("Delete")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var voo = await _context.Voos.FindAsync(id);
            if (voo != null)
            {
                _context.Voos.Remove(voo);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        // =============================================
        // MÉTODOS PRIVADOS AUXILIARES
        // =============================================

        private async Task CarregarViewBags()
        {
            try
            {
                // CORREÇÃO: Usar AsNoTracking para consultas que não modificam dados
                var aeroportos = await _context.Aeroportos
                    .AsNoTracking()
                    .OrderBy(a => a.Nome)
                    .ToListAsync();

                ViewBag.Aeroportos = aeroportos
                    .Select(a => new SelectListItem
                    {
                        Value = a.AeroportoId.ToString(),
                        Text = $"{a.Nome} ({a.CodigoIATA}) - {a.Cidade}"
                    })
                    .ToList();

                var aeronaves = await _context.Aeronaves
                    .AsNoTracking()
                    .OrderBy(a => a.TipoAeronave)
                    .ToListAsync();

                ViewBag.Aeronaves = aeronaves
                    .Select(a => new SelectListItem
                    {
                        Value = a.AeronaveId.ToString(),
                        Text = $"{a.TipoAeronave} - {a.NumeroPoltronas} poltronas"
                    })
                    .ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao carregar ViewBags: {ex.Message}");
                ViewBag.Aeroportos = new List<SelectListItem>();
                ViewBag.Aeronaves = new List<SelectListItem>();
            }
        }

        private async void ValidarVoo(Voo voo)
        {
            if (voo.AeroportoOrigemId == voo.AeroportoDestinoId)
            {
                ModelState.AddModelError("AeroportoDestinoId", "O aeroporto de destino deve ser diferente do aeroporto de origem.");
            }

            if (voo.HorarioChegadaPrevisto <= voo.HorarioSaida)
            {
                ModelState.AddModelError("HorarioChegadaPrevisto", "O horário de chegada deve ser posterior ao horário de saída.");
            }

            if (await _context.Voos.AnyAsync(v => v.NumeroVoo == voo.NumeroVoo))
            {
                ModelState.AddModelError("NumeroVoo", "Este número de voo já está cadastrado.");
            }
        }
    }
}