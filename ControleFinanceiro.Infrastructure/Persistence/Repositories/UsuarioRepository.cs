using Dapper;
using ControleFinanceiro.Domain.Entities;
using ControleFinanceiro.Domain.Interfaces;
using System.Data;
using ControleFinanceiro.Infrastructure.Persistence;

namespace ControleFinanceiro.Infrastructure.Persistence.Repositories;

public class UsuarioRepository : IUsuarioRepository
{
    private readonly DbSession _session;
    private readonly ISqlProvider _sqlProvider;

    public UsuarioRepository(DbSession session, ISqlProvider sqlProvider)
    {
        _session = session;
        _sqlProvider = sqlProvider;
    }

    public async Task<Usuario?> GetByIdAsync(int id)
    {
        var sql = _sqlProvider.GetSql("Usuario", "GetById");
        return await _session.Connection.QueryFirstOrDefaultAsync<Usuario>(sql, new { Id = id });
    }

    public async Task<Usuario?> GetByEmailAsync(string email)
    {
        var sql = _sqlProvider.GetSql("Usuario", "GetByEmail");
        return await _session.Connection.QueryFirstOrDefaultAsync<Usuario>(sql, new { Email = email });
    }

    public async Task<Usuario?> GetByUsernameAsync(string username)
    {
        var sql = _sqlProvider.GetSql("Usuario", "GetByUsername");
        return await _session.Connection.QueryFirstOrDefaultAsync<Usuario>(sql, new { Username = username });
    }

    public async Task AddAsync(Usuario usuario)
    {
        var sql = _sqlProvider.GetSql("Usuario", "Add");
        
        var id = await _session.Connection.ExecuteScalarAsync<int>(sql, new 
        { 
            usuario.Nome, 
            usuario.Username,
            usuario.Email, 
            usuario.PasswordHash, 
            usuario.CreatedAt 
        });

        usuario.Id = id;
    }
}
