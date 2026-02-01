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
}
