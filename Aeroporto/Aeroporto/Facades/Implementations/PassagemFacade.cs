using Microsoft.EntityFrameworkCore;
using SistemaAereo.Data.Context;
using SistemaAereo.Facades.Interfaces;
using SistemaAereo.Models.DTOs;
using SistemaAereo.Models.Entities;
using SistemaAereo.Models.Enums;
using SistemaAereo.Repositories;
using SistemaAereo.Repositories.Interfaces;
using SistemaAereo.Services.Interfaces;

namespace SistemaAereo.Facades.Implementations
{
    public class PassagemFacade : IPassagemFacade
    {
        private readonly IPassagemRepository _passagemRepository;
        private readonly IPoltronaRepository _poltronaRepository;
        private readonly IClientePreferencialRepository _clienteRepository;
        private readonly IVooRepository _vooRepository;
        private readonly IPoltronaService _poltronaService;
        private readonly AeroportoContext _context;
        private readonly ILogger<PassagemFacade> _logger;

        public PassagemFacade(
            IPassagemRepository passagemRepository,
            IPoltronaRepository poltronaRepository,
            IClientePreferencialRepository clienteRepository,
            IVooRepository vooRepository,
            IPoltronaService poltronaService,
            AeroportoContext context,
            ILogger<PassagemFacade> logger)
        {
            _passagemRepository = passagemRepository;
            _poltronaRepository = poltronaRepository;
            _clienteRepository = clienteRepository;
            _vooRepository = vooRepository;
            _poltronaService = poltronaService;
            _context = context;
            _logger = logger;
        }

        public async Task<PassagemResultDto> EmitirPassagemAsync(EmitirPassagemRequestDto request)
        {
            _logger.LogInformation($"Iniciando emissão de passagem - Cliente: {request.ClienteId}, Voo: {request.VooId}, Poltrona: {request.PoltronaId}");

            // Validações iniciais
            var cliente = await ValidarClienteAsync(request.ClienteId);
            if (cliente == null)
                return PassagemResultDto.Fail("Cliente não encontrado ou inativo");

            var voo = await ValidarVooAsync(request.VooId);
            if (voo == null)
                return PassagemResultDto.Fail("Voo não encontrado");

            if (voo.HorarioSaida < DateTime.Now)
                return PassagemResultDto.Fail("Não é possível emitir passagem para um voo que já partiu");

            // Iniciar transação
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                // Verificar e bloquear poltrona
                var poltrona = await ValidarPoltronaAsync(request.PoltronaId);
                if (poltrona == null)
                    return PassagemResultDto.Fail("Poltrona não encontrada ou indisponível");

                // Verificar dupla ocupação
                if (await PoltronaOcupadaAsync(poltrona.PoltronaId))
                    return PassagemResultDto.Fail("Poltrona já foi ocupada por outra passagem");

                // Marcar poltrona como indisponível
                poltrona.Disponivel = false;
                _context.Poltronas.Update(poltrona);
                await _context.SaveChangesAsync();

                // Criar passagem
                var passagem = CriarPassagem(request, poltrona);
                _context.Passagens.Add(passagem);
                await _context.SaveChangesAsync();

                await transaction.CommitAsync();

                _logger.LogInformation($"Passagem emitida com sucesso - Bilhete: {passagem.NumeroBilhete}");
                return PassagemResultDto.Ok(passagem);
            }
            catch (DbUpdateConcurrencyException ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Conflito de concorrência ao emitir passagem");
                return PassagemResultDto.Fail("A poltrona foi comprada por outro usuário. Tente novamente.");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Erro ao emitir passagem");
                return PassagemResultDto.Fail($"Erro ao processar a compra: {ex.Message}");
            }
        }

        public async Task<PassagemResultDto> CancelarPassagemAsync(CancelarPassagemRequestDto request)
        {
            _logger.LogInformation($"Iniciando cancelamento de passagem - ID: {request.PassagemId}");

            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var passagem = await _context.Passagens
                    .Include(p => p.Voo)
                    .FirstOrDefaultAsync(p => p.PassagemId == request.PassagemId);

                if (passagem == null)
                    return PassagemResultDto.Fail("Passagem não encontrada");

                if (passagem.Status == PassagemStatus.Cancelada)
                    return PassagemResultDto.Fail("Passagem já está cancelada");

                if (passagem.Status == PassagemStatus.Embarcada)
                    return PassagemResultDto.Fail("Não é possível cancelar uma passagem já embarcada");

                if (passagem.Voo != null && passagem.Voo.HorarioSaida < DateTime.Now)
                    return PassagemResultDto.Fail("Não é possível cancelar uma passagem de um voo que já partiu");

                // Atualizar status da passagem
                passagem.Status = PassagemStatus.Cancelada;
                _context.Passagens.Update(passagem);
                await _context.SaveChangesAsync();

                // Liberar poltrona
                var poltrona = await _poltronaRepository.GetByIdAsync(passagem.PoltronaId);
                if (poltrona != null)
                {
                    poltrona.Disponivel = true;
                    _context.Poltronas.Update(poltrona);
                    await _context.SaveChangesAsync();
                }

                await transaction.CommitAsync();

                _logger.LogInformation($"Passagem cancelada com sucesso - ID: {request.PassagemId}");
                return PassagemResultDto.CancelOk();
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, $"Erro ao cancelar passagem {request.PassagemId}");
                return PassagemResultDto.Fail($"Erro ao cancelar passagem: {ex.Message}");
            }
        }

        public async Task<PassagemResultDto> RealizarCheckinAsync(CheckinRequestDto request)
        {
            _logger.LogInformation($"Iniciando check-in - Passagem ID: {request.PassagemId}");

            try
            {
                var passagem = await _passagemRepository.GetPassagemCompletaAsync(request.PassagemId);
                if (passagem == null)
                    return PassagemResultDto.Fail("Passagem não encontrada");

                if (passagem.Status != PassagemStatus.Confirmada)
                    return PassagemResultDto.Fail($"Check-in não permitido. Status atual: {passagem.Status}");

                if (passagem.Voo != null && passagem.Voo.HorarioSaida < DateTime.Now)
                    return PassagemResultDto.Fail("Não é possível fazer check-in de um voo que já partiu");

                passagem.Status = PassagemStatus.CheckIn;
                await _passagemRepository.UpdateAsync(passagem);

                _logger.LogInformation($"Check-in realizado - Passagem ID: {request.PassagemId}");
                return PassagemResultDto.CheckinOk();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Erro ao realizar check-in da passagem {request.PassagemId}");
                return PassagemResultDto.Fail($"Erro ao realizar check-in: {ex.Message}");
            }
        }

        public async Task<PassagemResultDto> RegistrarEmbarqueAsync(int passagemId)
        {
            _logger.LogInformation($"Registrando embarque - Passagem ID: {passagemId}");

            try
            {
                var passagem = await _passagemRepository.GetByIdAsync(passagemId);
                if (passagem == null)
                    return PassagemResultDto.Fail("Passagem não encontrada");

                if (passagem.Status != PassagemStatus.CheckIn)
                    return PassagemResultDto.Fail($"Embarque não permitido. Status atual: {passagem.Status}. É necessário fazer check-in primeiro.");

                passagem.Status = PassagemStatus.Embarcada;
                await _passagemRepository.UpdateAsync(passagem);

                _logger.LogInformation($"Embarque registrado - Passagem ID: {passagemId}");
                return PassagemResultDto.EmbarqueOk();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Erro ao registrar embarque da passagem {passagemId}");
                return PassagemResultDto.Fail($"Erro ao registrar embarque: {ex.Message}");
            }
        }

        public async Task<Passagem> ObterPassagemCompletaAsync(int passagemId)
        {
            return await _context.Passagens
                .AsNoTracking()
                .Include(p => p.Voo)
                    .ThenInclude(v => v.AeroportoOrigem)
                .Include(p => p.Voo)
                    .ThenInclude(v => v.AeroportoDestino)
                .Include(p => p.Voo)
                    .ThenInclude(v => v.Aeronave)
                .Include(p => p.Cliente)
                .Include(p => p.Poltrona)
                .FirstOrDefaultAsync(p => p.PassagemId == passagemId);
        }

        public async Task<bool> VerificarDisponibilidadePoltronaAsync(int vooId, int poltronaId)
        {
            var poltrona = await _context.Poltronas
                .FirstOrDefaultAsync(p => p.PoltronaId == poltronaId && p.VooId == vooId);

            if (poltrona == null) return false;

            var ocupada = await _context.Passagens
                .AnyAsync(p => p.PoltronaId == poltronaId && p.Status != PassagemStatus.Cancelada);

            return poltrona.Disponivel && !ocupada;
        }

        // =============================================
        // MÉTODOS PRIVADOS AUXILIARES
        // =============================================

        private async Task<ClientePreferencial> ValidarClienteAsync(int clienteId)
        {
            return await _context.ClientesPreferenciais
                .FirstOrDefaultAsync(c => c.ClienteId == clienteId && c.Ativo);
        }

        private async Task<Voo> ValidarVooAsync(int vooId)
        {
            return await _context.Voos
                .FirstOrDefaultAsync(v => v.VooId == vooId);
        }

        private async Task<Poltrona> ValidarPoltronaAsync(int poltronaId)
        {
            return await _context.Poltronas
                .FirstOrDefaultAsync(p => p.PoltronaId == poltronaId && p.Disponivel);
        }

        private async Task<bool> PoltronaOcupadaAsync(int poltronaId)
        {
            return await _context.Passagens
                .AnyAsync(p => p.PoltronaId == poltronaId && p.Status != PassagemStatus.Cancelada);
        }

        private Passagem CriarPassagem(EmitirPassagemRequestDto request, Poltrona poltrona)
        {
            return new Passagem
            {
                ClienteId = request.ClienteId,
                VooId = request.VooId,
                PoltronaId = request.PoltronaId,
                NumeroBilhete = GerarNumeroBilhete(),
                DataEmissao = DateTime.Now,
                Preco = poltrona.Preco,
                Status = PassagemStatus.Confirmada,
                Classe = poltrona.Tipo
            };
        }

        private string GerarNumeroBilhete()
        {
            return Guid.NewGuid().ToString("N").Substring(0, 20).ToUpper();
        }
    }
}