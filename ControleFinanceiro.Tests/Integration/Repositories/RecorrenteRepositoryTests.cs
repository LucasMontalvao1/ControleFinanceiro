using ControleFinanceiro.Domain.Entities;
using ControleFinanceiro.Infrastructure.Persistence;
using ControleFinanceiro.Infrastructure.Persistence.Repositories;
using ControleFinanceiro.Tests.Integration.Infrastructure;
using FluentAssertions;
using MySql.Data.MySqlClient;
using Xunit;

namespace ControleFinanceiro.Tests.Integration.Repositories;

[Collection("Database")]
public class RecorrenteRepositoryTests
{
    private readonly DatabaseFixture _fixture;
    private readonly RecorrenteRepository _repository;
    private readonly UsuarioRepository _usuarioRepository;
    private readonly CategoriaRepository _categoriaRepository;
    private readonly DbSession _session;

    public RecorrenteRepositoryTests(DatabaseFixture fixture)
    {
        _fixture = fixture;
        var connection = new MySqlConnection(_fixture.ConnectionString);
        connection.Open();
        _session = new DbSession(new TestDbConnectionFactory(_fixture.ConnectionString));
        _repository = new RecorrenteRepository(_session, new EmbeddedSqlProvider());
        _usuarioRepository = new UsuarioRepository(_session, new EmbeddedSqlProvider());
        _categoriaRepository = new CategoriaRepository(_session, new EmbeddedSqlProvider());
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
            Nome = "Categoria Recorrente",
            Tipo = "Despesa",
            UsuarioId = usuario.Id
        });

        return (usuario.Id, categoriaId);
    }

    [Fact]
    public async Task DeveCriarRecorrente_ComSucesso()
    {
        var (usuarioId, categoriaId) = await CriarDadosTeste();

        var recorrente = new Recorrente
        {
            Descricao = "Netflix",
            Valor = 55.90m,
            Tipo = "Despesa",
            DiaVencimento = 15,
            CategoriaId = categoriaId,
            UsuarioId = usuarioId,
            Ativo = true
        };

        var id = await _repository.CreateAsync(recorrente);

        id.Should().BeGreaterThan(0);

        var retrieved = await _repository.GetByIdAsync(id, usuarioId);
        retrieved.Should().NotBeNull();
        retrieved!.Descricao.Should().Be("Netflix");
        retrieved.DiaVencimento.Should().Be(15);
    }

    [Fact]
    public async Task DeveListarRecorrentes_DoUsuario()
    {
        var (usuarioId, categoriaId) = await CriarDadosTeste();

        await _repository.CreateAsync(new Recorrente
        {
            Descricao = "Aluguel",
            Valor = 1500,
            Tipo = "Despesa",
            DiaVencimento = 5,
            CategoriaId = categoriaId,
            UsuarioId = usuarioId,
            Ativo = true
        });

        await _repository.CreateAsync(new Recorrente
        {
            Descricao = "Internet",
            Valor = 120,
            Tipo = "Despesa",
            DiaVencimento = 10,
            CategoriaId = categoriaId,
            UsuarioId = usuarioId,
            Ativo = true
        });

        var recorrentes = await _repository.GetAllAsync(usuarioId);

        recorrentes.Should().HaveCountGreaterOrEqualTo(2);
        recorrentes.Should().Contain(r => r.Descricao == "Aluguel");
        recorrentes.Should().Contain(r => r.Descricao == "Internet");
    }

    [Fact]
    public async Task DeveAtualizarRecorrente_ComSucesso()
    {
        var (usuarioId, categoriaId) = await CriarDadosTeste();

        var recorrente = new Recorrente
        {
            Descricao = "Spotify",
            Valor = 20,
            Tipo = "Despesa",
            DiaVencimento = 20,
            CategoriaId = categoriaId,
            UsuarioId = usuarioId,
            Ativo = true
        };

        var id = await _repository.CreateAsync(recorrente);
        recorrente.Id = id;

        recorrente.Valor = 25;
        await _repository.UpdateAsync(recorrente);

        var updated = await _repository.GetByIdAsync(id, usuarioId);
        updated.Should().NotBeNull();
        updated!.Valor.Should().Be(25);
    }

    [Fact]
    public async Task DeveExcluirRecorrente_ComSucesso()
    {
        var (usuarioId, categoriaId) = await CriarDadosTeste();

        var recorrente = new Recorrente
        {
            Descricao = "Excluir",
            Valor = 50,
            Tipo = "Despesa",
            DiaVencimento = 1,
            CategoriaId = categoriaId,
            UsuarioId = usuarioId,
            Ativo = true
        };

        var id = await _repository.CreateAsync(recorrente);

        await _repository.DeleteAsync(id, usuarioId);

        var deleted = await _repository.GetByIdAsync(id, usuarioId);
        deleted.Should().BeNull();
    }
}
