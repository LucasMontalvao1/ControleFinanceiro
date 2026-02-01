using ControleFinanceiro.Domain.Entities;

namespace ControleFinanceiro.Domain.Interfaces;

public interface ILancamentoRepository
{
    Task<IEnumerable<Lancamento>> GetAllByUsuarioIdAsync(int usuarioId, DateTime start, DateTime end);
    Task<Lancamento?> GetByIdAsync(int id, int usuarioId);
    Task<int> AddAsync(Lancamento lancamento);
    Task UpdateAsync(Lancamento lancamento);
    Task DeleteAsync(int id, int usuarioId);
    Task<(decimal TotalEntradas, decimal TotalSaidas)> GetSummaryAsync(int usuarioId, DateTime start, DateTime end);
}
