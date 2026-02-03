using System.Collections.Generic;
using System.Threading.Tasks;
using ControleFinanceiro.Domain.Entities;
using ControleFinanceiro.Domain.Interfaces;
using Dapper;

namespace ControleFinanceiro.Infrastructure.Persistence.Repositories;

public class MetaRepository : IMetaRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public MetaRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<IEnumerable<Meta>> GetAllAsync(int usuarioId, int mes, int ano)
    {
        using var conn = _connectionFactory.CreateConnection();
        const string sql = @"
            SELECT m.*, c.Nome as CategoriaNome 
            FROM Metas m
            JOIN Categorias c ON m.CategoriaId = c.Id
            WHERE m.UsuarioId = @UsuarioId AND m.Mes = @Mes AND m.Ano = @Ano";
        return await conn.QueryAsync<Meta>(sql, new { UsuarioId = usuarioId, Mes = mes, Ano = ano });
    }

    public async Task<Meta?> GetByIdAsync(int id, int usuarioId)
    {
        using var conn = _connectionFactory.CreateConnection();
        const string sql = "SELECT * FROM Metas WHERE Id = @Id AND UsuarioId = @UsuarioId";
        return await conn.QueryFirstOrDefaultAsync<Meta>(sql, new { Id = id, UsuarioId = usuarioId });
    }

    public async Task<int> CreateAsync(Meta meta)
    {
        using var conn = _connectionFactory.CreateConnection();
        const string sql = @"
            INSERT INTO Metas (UsuarioId, CategoriaId, ValorLimite, Mes, Ano)
            VALUES (@UsuarioId, @CategoriaId, @ValorLimite, @Mes, @Ano);
            SELECT LAST_INSERT_ID();";
        return await conn.ExecuteScalarAsync<int>(sql, meta);
    }

    public async Task UpdateAsync(Meta meta)
    {
        using var conn = _connectionFactory.CreateConnection();
        const string sql = @"
            UPDATE Metas 
            SET CategoriaId = @CategoriaId, 
                ValorLimite = @ValorLimite, 
                Mes = @Mes, 
                Ano = @Ano
            WHERE Id = @Id AND UsuarioId = @UsuarioId";
        await conn.ExecuteAsync(sql, meta);
    }

    public async Task DeleteAsync(int id, int usuarioId)
    {
        using var conn = _connectionFactory.CreateConnection();
        const string sql = "DELETE FROM Metas WHERE Id = @Id AND UsuarioId = @UsuarioId";
        await conn.ExecuteAsync(sql, new { Id = id, UsuarioId = usuarioId });
    }
}
