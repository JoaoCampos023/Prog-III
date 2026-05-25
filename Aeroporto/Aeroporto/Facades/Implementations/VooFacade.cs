using Microsoft.EntityFrameworkCore;
using SistemaAereo.Data.Context;
using SistemaAereo.Facades.Interfaces;
using SistemaAereo.Models.DTOs;
using SistemaAereo.Models.Entities;
using SistemaAereo.Models.Enums;
using SistemaAereo.Repositories.Interfaces;
using SistemaAereo.Services.Interfaces;

namespace SistemaAereo.Facades.Implementations
{
    public class VooFacade : IVooFacade
    {
        private readonly IVooRepository _vooRepository;
        private readonly IPoltronaService _poltronaService;
        private readonly AeroportoContext _context;
        private readonly ILogger<VooFacade> _logger;

        public VooFacade(
            IVooRepository vooRepository,
            IPoltronaService poltronaService,
            AeroportoContext context,
            ILogger<VooFacade> logger)
        {
            _vooRepository = vooRepository;
            _poltronaService = poltronaService;
            _context = context;
            _logger = logger;
        }

        public async Task<VooResultDto> CriarVooAsync(Voo voo)
        {
            _logger.LogInformation($"Iniciando criação do voo - Número: {voo.NumeroVoo}");

            // Validações
            if (voo.AeroportoOrigemId == voo.AeroportoDestinoId)
                return VooResultDto.Fail("O aeroporto de destino deve ser diferente do aeroporto de origem.");

            if (voo.HorarioChegadaPrevisto <= voo.HorarioSaida)
                return VooResultDto.Fail("O horário de chegada deve ser posterior ao horário de saída.");

            if (voo.HorarioSaida < DateTime.Now)
                return VooResultDto.Fail("Não é possível cadastrar um voo com data/hora no passado.");

            var numeroExiste = await _vooRepository.NumeroVooExistsAsync(voo.NumeroVoo);
            if (numeroExiste)
                return VooResultDto.Fail("Este número de voo já está cadastrado.");

            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                // Salvar voo
                _context.Voos.Add(voo);
                await _context.SaveChangesAsync();

                // Criar poltronas automaticamente
                await _poltronaService.CriarPoltronasParaVooAsync(voo.VooId);

                await transaction.CommitAsync();

                _logger.LogInformation($"Voo criado com sucesso - ID: {voo.VooId}, Número: {voo.NumeroVoo}");
                return VooResultDto.Ok(voo, "Voo cadastrado com sucesso!");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, $"Erro ao criar voo {voo.NumeroVoo}");
                return VooResultDto.Fail($"Erro ao criar voo: {ex.Message}");
            }
        }

        public async Task<VooResultDto> AtualizarVooAsync(Voo voo)
        {
            _logger.LogInformation($"Iniciando atualização do voo - ID: {voo.VooId}");

            if (voo.HorarioSaida < DateTime.Now)
                return VooResultDto.Fail("Não é possível editar para uma data/hora no passado.");

            if (voo.AeroportoOrigemId == voo.AeroportoDestinoId)
                return VooResultDto.Fail("O aeroporto de destino deve ser diferente do aeroporto de origem.");

            if (voo.HorarioChegadaPrevisto <= voo.HorarioSaida)
                return VooResultDto.Fail("O horário de chegada deve ser posterior ao horário de saída.");

            var numeroExiste = await _vooRepository.NumeroVooExistsAsync(voo.NumeroVoo, voo.VooId);
            if (numeroExiste)
                return VooResultDto.Fail("Este número de voo já está cadastrado.");

            try
            {
                _context.Voos.Update(voo);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Voo atualizado com sucesso - ID: {voo.VooId}");
                return VooResultDto.Ok(voo, "Voo atualizado com sucesso!");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Erro ao atualizar voo {voo.VooId}");
                return VooResultDto.Fail($"Erro ao atualizar voo: {ex.Message}");
            }
        }

        public async Task<VooResultDto> ExcluirVooAsync(int vooId)
        {
            _logger.LogInformation($"Iniciando exclusão do voo - ID: {vooId}");

            var voo = await _context.Voos
                .Include(v => v.Poltronas)
                .Include(v => v.Passagens)
                .FirstOrDefaultAsync(v => v.VooId == vooId);

            if (voo == null)
                return VooResultDto.Fail("Voo não encontrado");

            var temPassagens = voo.Passagens.Any(p => p.Status != PassagemStatus.Cancelada);
            if (temPassagens)
                return VooResultDto.Fail("Não é possível excluir o voo pois existem passagens vendidas.");

            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                if (voo.Poltronas.Any())
                    _context.Poltronas.RemoveRange(voo.Poltronas);

                var escalas = await _context.Escalas.Where(e => e.VooId == vooId).ToListAsync();
                if (escalas.Any())
                    _context.Escalas.RemoveRange(escalas);

                _context.Voos.Remove(voo);
                await _context.SaveChangesAsync();

                await transaction.CommitAsync();

                _logger.LogInformation($"Voo excluído com sucesso - ID: {vooId}");
                return VooResultDto.Ok(voo, "Voo excluído com sucesso!");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, $"Erro ao excluir voo {vooId}");
                return VooResultDto.Fail($"Erro ao excluir voo: {ex.Message}");
            }
        }

        public async Task<VooResultDto> RecriarPoltronasAsync(int vooId)
        {
            _logger.LogInformation($"Recriando poltronas do voo - ID: {vooId}");

            var voo = await _context.Voos
                .Include(v => v.Poltronas)
                .FirstOrDefaultAsync(v => v.VooId == vooId);

            if (voo == null)
                return VooResultDto.Fail("Voo não encontrado");

            var temPassagens = await _context.Passagens
                .AnyAsync(p => p.VooId == vooId && p.Status != PassagemStatus.Cancelada);

            if (temPassagens)
                return VooResultDto.Fail("Não é possível recriar poltronas pois existem passagens vendidas para este voo.");

            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                if (voo.Poltronas.Any())
                {
                    _context.Poltronas.RemoveRange(voo.Poltronas);
                    await _context.SaveChangesAsync();
                }

                await _poltronaService.CriarPoltronasParaVooAsync(vooId);

                await transaction.CommitAsync();

                _logger.LogInformation($"Poltronas recriadas com sucesso - Voo ID: {vooId}");
                return VooResultDto.Ok(voo, "Poltronas recriadas com sucesso!");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, $"Erro ao recriar poltronas do voo {vooId}");
                return VooResultDto.Fail($"Erro ao recriar poltronas: {ex.Message}");
            }
        }

        public async Task<VooEstatisticasDto> ObterEstatisticasVooAsync(int vooId)
        {
            var voo = await _context.Voos
                .Include(v => v.Poltronas)
                .FirstOrDefaultAsync(v => v.VooId == vooId);

            if (voo == null)
                return null;

            var totalPoltronas = voo.Poltronas.Count;
            var poltronasDisponiveis = voo.Poltronas.Count(p => p.Disponivel);
            var poltronasOcupadas = totalPoltronas - poltronasDisponiveis;

            var totalPassagens = await _context.Passagens
                .CountAsync(p => p.VooId == vooId && p.Status != PassagemStatus.Cancelada);

            var faturamento = await _context.Passagens
                .Where(p => p.VooId == vooId && p.Status != PassagemStatus.Cancelada)
                .SumAsync(p => p.Preco);

            var percentualOcupacao = totalPoltronas > 0
                ? (double)poltronasOcupadas / totalPoltronas * 100
                : 0;

            return new VooEstatisticasDto
            {
                TotalPoltronas = totalPoltronas,
                PoltronasDisponiveis = poltronasDisponiveis,
                PoltronasOcupadas = poltronasOcupadas,
                TotalPassagens = totalPassagens,
                FaturamentoTotal = faturamento,
                PercentualOcupacao = Math.Round(percentualOcupacao, 2)
            };
        }
    }
}