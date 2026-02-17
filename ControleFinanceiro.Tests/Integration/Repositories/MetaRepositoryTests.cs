using ControleFinanceiro.Domain.Entities;
using ControleFinanceiro.Infrastructure.Persistence;
using ControleFinanceiro.Infrastructure.Persistence.Repositories;
using ControleFinanceiro.Tests.Integration.Infrastructure;
using FluentAssertions;
using MySql.Data.MySqlClient;
using Xunit;

namespace ControleFinanceiro.Tests.Integration.Repositories;

[Collection("Database")]
public class MetaRepositoryTests
{
    private readonly DatabaseFixture _fixture;
    private readonly MetaRepository _repository;
    private readonly UsuarioRepository _usuarioRepository;
    private readonly CategoriaRepository _categoriaRepository;
    private readonly TestDbConnectionFactory _connectionFactory;

    public MetaRepositoryTests(DatabaseFixture fixture)
    {
        _fixture = fixture;
        var connection = new MySqlConnection(_fixture.ConnectionString);
        connection.Open();
        
        _connectionFactory = new TestDbConnectionFactory(_fixture.ConnectionString);
        // MetaRepository uses IDbConnectionFactory directly, not DbSession/ISqlProvider
        _repository = new MetaRepository(_connectionFactory);
        
        var session = new DbSession(_connectionFactory);
        var provider = new EmbeddedSqlProvider();
        _usuarioRepository = new UsuarioRepository(session, provider);
        _categoriaRepository = new CategoriaRepository(session, provider);
    }

    private async Task<(int usuarioId, int categoriaId)> CriarDadosTeste()
    {
        var usuario = new Usuario
        {
            Nome = "Test User",
            Username = $"user_{Guid.NewGuid():N}",
            Email = $"user_{Guid.NewGuid():N}@test.com",
            PasswordHash = "hash"
        };
        await _usuarioRepository.AddAsync(usuario);

        var categoriaId = await _categoriaRepository.AddAsync(new Categoria
        {
            Nome = "Categoria Meta",
            Tipo = "Despesa",
            UsuarioId = usuario.Id
        });

        return (usuario.Id, categoriaId);
    }

    [Fact]
    public async Task DeveCriarMeta_ComSucesso()
    {
        var (usuarioId, categoriaId) = await CriarDadosTeste();

        var meta = new Meta
        {
            CategoriaId = categoriaId,
            ValorLimite = 1000,
            Mes = 3,
            Ano = 2024,
            UsuarioId = usuarioId
        };

        var id = await _repository.CreateAsync(meta);

        id.Should().BeGreaterThan(0);

        var retrieved = await _repository.GetByIdAsync(id, usuarioId);
        retrieved.Should().NotBeNull();
        retrieved!.ValorLimite.Should().Be(1000);
        retrieved.Mes.Should().Be(3);
    }

    [Fact]
    public async Task DeveListarMetas_DoUsuarioEMes()
    {
        var (usuarioId, categoriaId) = await CriarDadosTeste();

        await _repository.CreateAsync(new Meta 
        { 
            CategoriaId = categoriaId, 
            ValorLimite = 500, 
            Mes = 4, 
            Ano = 2024, 
            UsuarioId = usuarioId 
        });

        // Meta de outro mês (não deve retornar)
        await _repository.CreateAsync(new Meta 
        { 
            CategoriaId = categoriaId, 
            ValorLimite = 600, 
            Mes = 5, 
            Ano = 2024, 
            UsuarioId = usuarioId 
        });

        var metas = await _repository.GetAllAsync(usuarioId, 4, 2024);

        metas.Should().HaveCount(1);
        metas.First().ValorLimite.Should().Be(500);
    }

    [Fact]
    public async Task DeveAtualizarMeta_ComSucesso()
    {
        var (usuarioId, categoriaId) = await CriarDadosTeste();

        var meta = new Meta
        {
            CategoriaId = categoriaId,
            ValorLimite = 1000,
            Mes = 3,
            Ano = 2024,
            UsuarioId = usuarioId
        };

        var id = await _repository.CreateAsync(meta);
        meta.Id = id;

        meta.ValorLimite = 1500;
        await _repository.UpdateAsync(meta);

        var updated = await _repository.GetByIdAsync(id, usuarioId);
        updated.Should().NotBeNull();
        updated!.ValorLimite.Should().Be(1500);
    }

    [Fact]
    public async Task DeveExcluirMeta_ComSucesso()
    {
        var (usuarioId, categoriaId) = await CriarDadosTeste();

        var meta = new Meta
        {
            CategoriaId = categoriaId,
            ValorLimite = 1000,
            Mes = 3,
            Ano = 2024,
            UsuarioId = usuarioId
        };

        var id = await _repository.CreateAsync(meta);

        await _repository.DeleteAsync(id, usuarioId);

        var deleted = await _repository.GetByIdAsync(id, usuarioId);
        deleted.Should().BeNull();
    }
}
