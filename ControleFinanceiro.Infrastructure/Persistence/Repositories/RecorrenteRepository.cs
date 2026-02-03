using Dapper;
using ControleFinanceiro.Domain.Entities;
using ControleFinanceiro.Domain.Interfaces;

namespace ControleFinanceiro.Infrastructure.Persistence.Repositories;

public class RecorrenteRepository : IRecorrenteRepository
{
    private readonly DbSession _session;
    private readonly ISqlProvider _sqlProvider;

    public RecorrenteRepository(DbSession session, ISqlProvider sqlProvider)
    {
        _session = session;
        _sqlProvider = sqlProvider;
    }

    public async Task<IEnumerable<Recorrente>> GetAllAsync(int usuarioId)
    {
        var sql = _sqlProvider.GetSql("Recorrente", "GetAll");
        return await _session.Connection.QueryAsync<Recorrente>(sql, new { UsuarioId = usuarioId });
    }

    public async Task<Recorrente?> GetByIdAsync(int id, int usuarioId)
    {
        var sql = _sqlProvider.GetSql("Recorrente", "GetById");
        return await _session.Connection.QueryFirstOrDefaultAsync<Recorrente>(sql, new { Id = id, UsuarioId = usuarioId });
    }

    public async Task<int> CreateAsync(Recorrente recorrente)
    {
        var sql = _sqlProvider.GetSql("Recorrente", "Add");
        return await _session.Connection.ExecuteScalarAsync<int>(sql, recorrente);
    }

    public async Task UpdateAsync(Recorrente recorrente)
    {
        var sql = _sqlProvider.GetSql("Recorrente", "Update");
        await _session.Connection.ExecuteAsync(sql, recorrente);
    }

    public async Task DeleteAsync(int id, int usuarioId)
    {
        var sql = _sqlProvider.GetSql("Recorrente", "Delete");
        await _session.Connection.ExecuteAsync(sql, new { Id = id, UsuarioId = usuarioId });
    }
}
