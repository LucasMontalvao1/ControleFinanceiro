using Dapper;
using ControleFinanceiro.Domain.Entities;
using ControleFinanceiro.Domain.Interfaces;
using ControleFinanceiro.Infrastructure.Persistence;

namespace ControleFinanceiro.Infrastructure.Persistence.Repositories;

public class LancamentoRepository : ILancamentoRepository
{
    private readonly DbSession _session;
    private readonly ISqlProvider _sqlProvider;

    public LancamentoRepository(DbSession session, ISqlProvider sqlProvider)
    {
        _session = session;
        _sqlProvider = sqlProvider;
    }

    public async Task<IEnumerable<Lancamento>> GetAllByUsuarioIdAsync(int usuarioId, DateTime start, DateTime end)
    {
        var sql = _sqlProvider.GetSql("Lancamento", "GetAllByUsuarioId");
        return await _session.Connection.QueryAsync<Lancamento>(sql, new { UsuarioId = usuarioId, Start = start, End = end });
    }

    public async Task<Lancamento?> GetByIdAsync(int id, int usuarioId)
    {
        var sql = _sqlProvider.GetSql("Lancamento", "GetById");
        return await _session.Connection.QueryFirstOrDefaultAsync<Lancamento>(sql, new { Id = id, UsuarioId = usuarioId });
    }

    public async Task<int> AddAsync(Lancamento lancamento)
    {
        var sql = _sqlProvider.GetSql("Lancamento", "Add");
        return await _session.Connection.ExecuteScalarAsync<int>(sql, lancamento);
    }

    public async Task UpdateAsync(Lancamento lancamento)
    {
        var sql = _sqlProvider.GetSql("Lancamento", "Update");
        await _session.Connection.ExecuteAsync(sql, lancamento);
    }

    public async Task DeleteAsync(int id, int usuarioId)
    {
        var sql = _sqlProvider.GetSql("Lancamento", "Delete");
        await _session.Connection.ExecuteAsync(sql, new { Id = id, UsuarioId = usuarioId });
    }

    public async Task<(decimal TotalEntradas, decimal TotalSaidas)> GetSummaryAsync(int usuarioId, DateTime start, DateTime end)
    {
        var sql = _sqlProvider.GetSql("Lancamento", "GetSummary");
        var result = await _session.Connection.QueryFirstOrDefaultAsync<dynamic>(sql, new { UsuarioId = usuarioId, Start = start, End = end });

        return (
            (decimal)(result?.TotalEntradas ?? 0m),
            (decimal)(result?.TotalSaidas ?? 0m)
        );
    }

    public async Task<IEnumerable<(string Categoria, decimal Valor)>> GetExpensesByCategoryAsync(int usuarioId, DateTime start, DateTime end)
    {
        var sql = _sqlProvider.GetSql("Lancamento", "GetExpensesByCategory");
        var results = await _session.Connection.QueryAsync<dynamic>(sql, new { UsuarioId = usuarioId, Start = start, End = end });
        return results.Select(r => ((string)r.Categoria, (decimal)r.Valor));
    }

    public async Task<IEnumerable<(DateTime Data, decimal Entradas, decimal Saidas)>> GetMonthlyEvolutionAsync(int usuarioId, DateTime start, DateTime end)
    {
        var sql = _sqlProvider.GetSql("Lancamento", "GetMonthlyEvolution");
        var results = await _session.Connection.QueryAsync<dynamic>(sql, new { UsuarioId = usuarioId, Start = start, End = end });
        return results.Select(r => ((DateTime)r.Data, (decimal)r.Entradas, (decimal)r.Saidas));
    }

    public async Task<IEnumerable<(string Mes, decimal Entradas, decimal Saidas, decimal Saldo)>> GetYearlyEvolutionAsync(int usuarioId, DateTime end)
    {
        var sql = _sqlProvider.GetSql("Lancamento", "GetYearlyEvolution");
        var results = await _session.Connection.QueryAsync<dynamic>(sql, new { UsuarioId = usuarioId, End = end });
        return results.Select(r => ((string)r.Mes, (decimal)r.Entradas, (decimal)r.Saidas, (decimal)r.Saldo));
    }

    public async Task<IEnumerable<Lancamento>> GetRecentAsync(int usuarioId, int take, DateTime? start = null, DateTime? end = null)
    {
        var sql = _sqlProvider.GetSql("Lancamento", "GetRecent");
        
        return await _session.Connection.QueryAsync<Lancamento>(sql, new { 
            UsuarioId = usuarioId, 
            Take = take,
            Start = start ?? new DateTime(2000, 1, 1),
            End = end ?? new DateTime(2100, 12, 31)
        });
    }
}
