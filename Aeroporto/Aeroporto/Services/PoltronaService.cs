using Microsoft.EntityFrameworkCore;
using SistemaAereo.Data.Context;
using SistemaAereo.Models.Entities;
using SistemaAereo.Models.Enums;
using SistemaAereo.Services.Interfaces;

namespace SistemaAereo.Services
{
    public class PoltronaService : IPoltronaService
    {
        private readonly AeroportoContext _context;
        private readonly ILogger<PoltronaService> _logger;

        public PoltronaService(AeroportoContext context, ILogger<PoltronaService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<List<Poltrona>> CriarPoltronasParaVooAsync(int vooId, int? numeroPoltronas = null)
        {
            try
            {
                var voo = await _context.Voos
                    .Include(v => v.Aeronave)
                    .FirstOrDefaultAsync(v => v.VooId == vooId);

                if (voo == null)
                {
                    _logger.LogWarning($"Voo {vooId} não encontrado para criação de poltronas");
                    return new List<Poltrona>();
                }

                // Verificar se já existem poltronas
                var poltronasExistentes = await _context.Poltronas.AnyAsync(p => p.VooId == vooId);
                if (poltronasExistentes)
                {
                    _logger.LogInformation($"Voo {vooId} já possui poltronas cadastradas");
                    return await _context.Poltronas.Where(p => p.VooId == vooId).ToListAsync();
                }

                int totalPoltronas = numeroPoltronas ?? voo.Aeronave?.NumeroPoltronas ?? 50;
                var poltronas = new List<Poltrona>();
                var random = new Random();

                for (int i = 1; i <= totalPoltronas; i++)
                {
                    var fileira = (i - 1) / 6 + 1;
                    var assento = (i - 1) % 6 + 1;
                    var letraAssento = ((char)('A' + (assento - 1))).ToString();

                    // Definir tipo baseado na posição
                    string tipo;
                    if (i <= totalPoltronas * 0.05)
                        tipo = PoltronaTipo.Primeira;
                    else if (i <= totalPoltronas * 0.2)
                        tipo = PoltronaTipo.Executiva;
                    else
                        tipo = PoltronaTipo.Economica;

                    // Definir localização baseada no assento
                    string localizacao = assento switch
                    {
                        1 or 6 => PoltronaLocalizacao.Janela,
                        2 or 5 => PoltronaLocalizacao.Meio,
                        3 or 4 => PoltronaLocalizacao.Corredor,
                        _ => PoltronaLocalizacao.Corredor
                    };

                    // Definir preço baseado no tipo
                    decimal preco = tipo switch
                    {
                        PoltronaTipo.Primeira => 800.00m,
                        PoltronaTipo.Executiva => 500.00m,
                        _ => 300.00m
                    };

                    // Adicionar variação aleatória
                    preco += random.Next(-50, 51);

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

                await _context.Poltronas.AddRangeAsync(poltronas);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Criadas {poltronas.Count} poltronas para o voo {voo.NumeroVoo}");
                return poltronas;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Erro ao criar poltronas para voo {vooId}");
                throw;
            }
        }

        public async Task<bool> ValidarPoltronasExistemAsync(int vooId)
        {
            return await _context.Poltronas.AnyAsync(p => p.VooId == vooId);
        }

        public async Task<int> GetTotalPoltronasDisponiveisAsync(int vooId)
        {
            return await _context.Poltronas
                .AsNoTracking()
                .CountAsync(p => p.VooId == vooId && p.Disponivel);
        }

        public async Task<int> GetTotalPoltronasOcupadasAsync(int vooId)
        {
            return await _context.Poltronas
                .AsNoTracking()
                .CountAsync(p => p.VooId == vooId && !p.Disponivel);
        }
    }
}