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
    Task<IEnumerable<(string Categoria, decimal Valor)>> GetExpensesByCategoryAsync(int usuarioId, DateTime start, DateTime end);
    Task<IEnumerable<(DateTime Data, decimal Entradas, decimal Saidas)>> GetMonthlyEvolutionAsync(int usuarioId, DateTime start, DateTime end);
    Task<IEnumerable<(string Mes, decimal Entradas, decimal Saidas, decimal Saldo)>> GetYearlyEvolutionAsync(int usuarioId, DateTime end);
    Task<IEnumerable<Lancamento>> GetRecentAsync(int usuarioId, int take);
}
