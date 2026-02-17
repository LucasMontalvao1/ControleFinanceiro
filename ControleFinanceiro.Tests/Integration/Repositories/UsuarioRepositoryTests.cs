using ControleFinanceiro.Domain.Entities;
using ControleFinanceiro.Infrastructure.Persistence;
using ControleFinanceiro.Infrastructure.Persistence.Repositories;
using ControleFinanceiro.Tests.Integration.Infrastructure;
using FluentAssertions;
using MySql.Data.MySqlClient;
using Xunit;

namespace ControleFinanceiro.Tests.Integration.Repositories;

[Collection("Database")]
public class UsuarioRepositoryTests
{
    private readonly DatabaseFixture _fixture;
    private readonly UsuarioRepository _repository;
    private readonly DbSession _session;

    public UsuarioRepositoryTests(DatabaseFixture fixture)
    {
        _fixture = fixture;
        var connection = new MySqlConnection(_fixture.ConnectionString);
        connection.Open();
        _session = new DbSession(new TestDbConnectionFactory(_fixture.ConnectionString));
        _repository = new UsuarioRepository(_session, new EmbeddedSqlProvider());
    }

    [Fact]
    public async Task DeveCriarUsuario_ComSucesso()
    {
        var usuario = new Usuario
        {
            Nome = "João Silva",
            Username = $"joao_{Guid.NewGuid():N}",
            Email = $"joao_{Guid.NewGuid():N}@example.com",
            PasswordHash = "hashed_password_123"
        };

        await _repository.AddAsync(usuario);

        usuario.Id.Should().BeGreaterThan(0);

        var retrieved = await _repository.GetByIdAsync(usuario.Id);
        retrieved.Should().NotBeNull();
        retrieved!.Nome.Should().Be("João Silva");
        retrieved.Email.Should().Be(usuario.Email);
    }

    [Fact]
    public async Task DeveBuscarUsuario_PorEmail()
    {
        var email = $"maria_{Guid.NewGuid():N}@example.com";
        var usuario = new Usuario
        {
            Nome = "Maria Santos",
            Username = $"maria_{Guid.NewGuid():N}",
            Email = email,
            PasswordHash = "password_hash"
        };

        await _repository.AddAsync(usuario);

        var result = await _repository.GetByEmailAsync(email);

        result.Should().NotBeNull();
        result!.Email.Should().Be(email);
        result.Nome.Should().Be("Maria Santos");
    }

    [Fact]
    public async Task DeveBuscarUsuario_PorUsername()
    {
        var username = $"pedro_{Guid.NewGuid():N}";
        var usuario = new Usuario
        {
            Nome = "Pedro Oliveira",
            Username = username,
            Email = $"pedro_{Guid.NewGuid():N}@example.com",
            PasswordHash = "password_hash"
        };

        await _repository.AddAsync(usuario);

        var result = await _repository.GetByUsernameAsync(username);

        result.Should().NotBeNull();
        result!.Username.Should().Be(username);
        result.Nome.Should().Be("Pedro Oliveira");
    }

    [Fact]
    public async Task DeveRetornarNull_QuandoEmailNaoExistir()
    {
        var result = await _repository.GetByEmailAsync("naoexiste@example.com");

        result.Should().BeNull();
    }

    [Fact]
    public async Task DeveRetornarNull_QuandoUsernameNaoExistir()
    {
        var result = await _repository.GetByUsernameAsync("usuarionaoexiste");

        result.Should().BeNull();
    }
}
