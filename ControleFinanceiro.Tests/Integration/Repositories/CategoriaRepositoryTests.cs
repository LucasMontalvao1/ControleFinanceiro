using ControleFinanceiro.Domain.Entities;
using ControleFinanceiro.Infrastructure.Persistence;
using ControleFinanceiro.Infrastructure.Persistence.Repositories;
using ControleFinanceiro.Tests.Integration.Infrastructure;
using FluentAssertions;
using MySql.Data.MySqlClient;
using Xunit;

namespace ControleFinanceiro.Tests.Integration.Repositories;

[Collection("Database")]
public class CategoriaRepositoryTests
{
    private readonly DatabaseFixture _fixture;
    private readonly CategoriaRepository _repository;
    private readonly UsuarioRepository _usuarioRepository;
    private readonly DbSession _session;

    public CategoriaRepositoryTests(DatabaseFixture fixture)
    {
        _fixture = fixture;
        var connection = new MySqlConnection(_fixture.ConnectionString);
        connection.Open();
        _session = new DbSession(new TestDbConnectionFactory(_fixture.ConnectionString));
        _repository = new CategoriaRepository(_session, new EmbeddedSqlProvider());
        _usuarioRepository = new UsuarioRepository(_session, new EmbeddedSqlProvider());
    }

    private async Task<int> CriarUsuarioTeste()
    {
        var usuario = new Usuario
        {
            Nome = "Test User",
            Username = $"user_{Guid.NewGuid():N}",
            Email = $"user_{Guid.NewGuid():N}@test.com",
            PasswordHash = "hash"
        };
        await _usuarioRepository.AddAsync(usuario);
        return usuario.Id;
    }

    [Fact]
    public async Task DeveCriarCategoria_ComSucesso()
    {
        var usuarioId = await CriarUsuarioTeste();

        var categoria = new Categoria
        {
            Nome = "Alimentação",
            Tipo = "Despesa",
            UsuarioId = usuarioId,
            IsDefault = false
        };

        var id = await _repository.AddAsync(categoria);

        id.Should().BeGreaterThan(0);

        var retrieved = await _repository.GetByIdAsync(id, usuarioId);
        retrieved.Should().NotBeNull();
        retrieved!.Nome.Should().Be("Alimentação");
        retrieved.Tipo.Should().Be("Despesa");
    }

    [Fact]
    public async Task DeveListarCategorias_DoUsuario()
    {
        var usuarioId = await CriarUsuarioTeste();

        await _repository.AddAsync(new Categoria { Nome = "Transporte", Tipo = "Despesa", UsuarioId = usuarioId });
        await _repository.AddAsync(new Categoria { Nome = "Salário", Tipo = "Receita", UsuarioId = usuarioId });

        var categorias = await _repository.GetAllByUsuarioIdAsync(usuarioId);

        categorias.Should().HaveCountGreaterOrEqualTo(2);
        categorias.Should().Contain(c => c.Nome == "Transporte");
        categorias.Should().Contain(c => c.Nome == "Salário");
    }

    [Fact]
    public async Task DeveAtualizarCategoria_ComSucesso()
    {
        var usuarioId = await CriarUsuarioTeste();

        var categoria = new Categoria
        {
            Nome = "Lazer",
            Tipo = "Despesa",
            UsuarioId = usuarioId,
            IsDefault = false
        };

        var id = await _repository.AddAsync(categoria);
        categoria.Id = id;

        categoria.Nome = "Entretenimento";
        await _repository.UpdateAsync(categoria);

        var updated = await _repository.GetByIdAsync(id, usuarioId);
        updated.Should().NotBeNull();
        updated!.Nome.Should().Be("Entretenimento");
    }

    [Fact]
    public async Task DeveExcluirCategoria_ComSucesso()
    {
        var usuarioId = await CriarUsuarioTeste();

        var categoria = new Categoria
        {
            Nome = "Temporária",
            Tipo = "Despesa",
            UsuarioId = usuarioId
        };

        var id = await _repository.AddAsync(categoria);

        await _repository.DeleteAsync(id, usuarioId);

        var deleted = await _repository.GetByIdAsync(id, usuarioId);
        deleted.Should().BeNull();
    }

    [Fact]
    public async Task NaoDeveRetornarCategoria_DeOutroUsuario()
    {
        var usuario1Id = await CriarUsuarioTeste();
        var usuario2Id = await CriarUsuarioTeste();

        var categoria = new Categoria
        {
            Nome = "Privada",
            Tipo = "Despesa",
            UsuarioId = usuario1Id
        };

        var id = await _repository.AddAsync(categoria);

        var result = await _repository.GetByIdAsync(id, usuario2Id);
        result.Should().BeNull();
    }
}
