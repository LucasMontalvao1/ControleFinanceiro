using ControleFinanceiro.Domain.Entities;
using ControleFinanceiro.Infrastructure.Persistence;
using ControleFinanceiro.Infrastructure.Persistence.Repositories;
using ControleFinanceiro.Tests.Integration.Infrastructure;
using FluentAssertions;
using MySql.Data.MySqlClient;
using Xunit;

namespace ControleFinanceiro.Tests.Integration.Repositories;

[Collection("Database")]
public class LancamentoRepositoryTests
{
    private readonly DatabaseFixture _fixture;
    private readonly LancamentoRepository _repository;
    private readonly UsuarioRepository _usuarioRepository;
    private readonly CategoriaRepository _categoriaRepository;
    private readonly DbSession _session;

    public LancamentoRepositoryTests(DatabaseFixture fixture)
    {
        _fixture = fixture;
        var connection = new MySqlConnection(_fixture.ConnectionString);
        connection.Open();
        _session = new DbSession(new TestDbConnectionFactory(_fixture.ConnectionString));
        _repository = new LancamentoRepository(_session, new EmbeddedSqlProvider());
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
            Nome = "Test Categoria",
            Tipo = "Despesa",
            UsuarioId = usuario.Id
        });

        return (usuario.Id, categoriaId);
    }

    [Fact]
    public async Task DeveCriarLancamento_ComSucesso()
    {
        var (usuarioId, categoriaId) = await CriarDadosTeste();

        var lancamento = new Lancamento
        {
            Descricao = "Compra supermercado",
            Valor = 250.50m,
            Data = DateTime.Today,
            Tipo = "Despesa",
            CategoriaId = categoriaId,
            UsuarioId = usuarioId
        };

        var id = await _repository.AddAsync(lancamento);

        id.Should().BeGreaterThan(0);

        var retrieved = await _repository.GetByIdAsync(id, usuarioId);
        retrieved.Should().NotBeNull();
        retrieved!.Descricao.Should().Be("Compra supermercado");
        retrieved.Valor.Should().Be(250.50m);
    }

    [Fact]
    public async Task DeveListarLancamentos_PorPeriodo()
    {
        var (usuarioId, categoriaId) = await CriarDadosTeste();

        var start = new DateTime(2024, 1, 1);
        var end = new DateTime(2024, 1, 31);

        await _repository.AddAsync(new Lancamento
        {
            Descricao = "Lançamento Janeiro",
            Valor = 100,
            Data = new DateTime(2024, 1, 15),
            Tipo = "Despesa",
            CategoriaId = categoriaId,
            UsuarioId = usuarioId
        });

        await _repository.AddAsync(new Lancamento
        {
            Descricao = "Lançamento Fevereiro",
            Valor = 200,
            Data = new DateTime(2024, 2, 15),
            Tipo = "Despesa",
            CategoriaId = categoriaId,
            UsuarioId = usuarioId
        });

        var lancamentos = await _repository.GetAllByUsuarioIdAsync(usuarioId, start, end);

        lancamentos.Should().HaveCount(1);
        lancamentos.First().Descricao.Should().Be("Lançamento Janeiro");
    }

    [Fact]
    public async Task DeveAtualizarLancamento_ComSucesso()
    {
        var (usuarioId, categoriaId) = await CriarDadosTeste();

        var lancamento = new Lancamento
        {
            Descricao = "Original",
            Valor = 100,
            Data = DateTime.Today,
            Tipo = "Despesa",
            CategoriaId = categoriaId,
            UsuarioId = usuarioId
        };

        var id = await _repository.AddAsync(lancamento);
        lancamento.Id = id;

        lancamento.Descricao = "Atualizado";
        lancamento.Valor = 150;

        await _repository.UpdateAsync(lancamento);

        var updated = await _repository.GetByIdAsync(id, usuarioId);
        updated.Should().NotBeNull();
        updated!.Descricao.Should().Be("Atualizado");
        updated.Valor.Should().Be(150);
    }

    [Fact]
    public async Task DeveExcluirLancamento_ComSucesso()
    {
        var (usuarioId, categoriaId) = await CriarDadosTeste();

        var lancamento = new Lancamento
        {
            Descricao = "Para excluir",
            Valor = 50,
            Data = DateTime.Today,
            Tipo = "Despesa",
            CategoriaId = categoriaId,
            UsuarioId = usuarioId
        };

        var id = await _repository.AddAsync(lancamento);

        await _repository.DeleteAsync(id, usuarioId);

        var deleted = await _repository.GetByIdAsync(id, usuarioId);
        deleted.Should().BeNull();
    }

    [Fact]
    public async Task NaoDeveRetornarLancamento_DeOutroUsuario()
    {
        var (usuario1Id, categoriaId) = await CriarDadosTeste();
        var (usuario2Id, _) = await CriarDadosTeste();

        var lancamento = new Lancamento
        {
            Descricao = "Privado",
            Valor = 100,
            Data = DateTime.Today,
            Tipo = "Despesa",
            CategoriaId = categoriaId,
            UsuarioId = usuario1Id
        };

        var id = await _repository.AddAsync(lancamento);

        var result = await _repository.GetByIdAsync(id, usuario2Id);
        result.Should().BeNull();
    }
}
