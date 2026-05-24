using SistemaAereo.Models.Entities;

namespace SistemaAereo.Services.Interfaces
{
    public interface IPoltronaService
    {
        Task<List<Poltrona>> CriarPoltronasParaVooAsync(int vooId, int? numeroPoltronas = null);
        Task<bool> ValidarPoltronasExistemAsync(int vooId);
        Task<int> GetTotalPoltronasDisponiveisAsync(int vooId);
        Task<int> GetTotalPoltronasOcupadasAsync(int vooId);
    }
}