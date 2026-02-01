using Dapper;
using ControleFinanceiro.Domain.Entities;
using ControleFinanceiro.Domain.Interfaces;
using ControleFinanceiro.Infrastructure.Persistence;

namespace ControleFinanceiro.Infrastructure.Persistence.Repositories;

public class CategoriaRepository : ICategoriaRepository
{
    private readonly DbSession _session;
    private readonly ISqlProvider _sqlProvider;

    public CategoriaRepository(DbSession session, ISqlProvider sqlProvider)
    {
        _session = session;
        _sqlProvider = sqlProvider;
    }

    public async Task<IEnumerable<Categoria>> GetAllByUsuarioIdAsync(int usuarioId)
    {
        var sql = _sqlProvider.GetSql("Categoria", "GetAllByUsuarioId");
        return await _session.Connection.QueryAsync<Categoria>(sql, new { UsuarioId = usuarioId });
    }

    public async Task<Categoria?> GetByIdAsync(int id, int usuarioId)
    {
        var sql = _sqlProvider.GetSql("Categoria", "GetById");
        return await _session.Connection.QueryFirstOrDefaultAsync<Categoria>(sql, new { Id = id, UsuarioId = usuarioId });
    }

    public async Task<int> AddAsync(Categoria categoria)
    {
        var sql = _sqlProvider.GetSql("Categoria", "Add");
        
        return await _session.Connection.ExecuteScalarAsync<int>(sql, categoria);
    }

    public async Task UpdateAsync(Categoria categoria)
    {
        var sql = _sqlProvider.GetSql("Categoria", "Update");
        
        await _session.Connection.ExecuteAsync(sql, categoria);
    }

    public async Task DeleteAsync(int id, int usuarioId)
    {
        var sql = _sqlProvider.GetSql("Categoria", "Delete");
        await _session.Connection.ExecuteAsync(sql, new { Id = id, UsuarioId = usuarioId });
    }
}
