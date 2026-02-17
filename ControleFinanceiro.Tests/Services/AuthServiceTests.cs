using ControleFinanceiro.Application.DTOs;
using ControleFinanceiro.Application.Services;
using ControleFinanceiro.Domain.Entities;
using ControleFinanceiro.Domain.Interfaces;
using FluentAssertions;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace ControleFinanceiro.Tests.Services;

public class AuthServiceTests
{
    private readonly Mock<IUsuarioRepository> _mockRepository;
    private readonly IOptions<Application.Configuration.JwtSettings> _jwtSettings;
    private readonly AuthService _authService;

    public AuthServiceTests()
    {
        _mockRepository = new Mock<IUsuarioRepository>();
        _jwtSettings = Options.Create(new Application.Configuration.JwtSettings
        {
            Secret = "ThisIsAVerySecureSecretKeyForTestingPurposesOnly12345",
            Issuer = "TestIssuer",
            Audience = "TestAudience",
            ExpiryDays = 7
        });
        _authService = new AuthService(_mockRepository.Object, _jwtSettings);
    }

    [Fact]
    public async Task DeveRegistrarUsuarioComSucesso_QuandoEmailEUsernameForemUnicos()
    {
        var request = new RegisterRequest(
            Nome: "Test User",
            Username: "testuser",
            Email: "test@example.com",
            Password: "SecurePass123"
        );

        _mockRepository.Setup(r => r.GetByEmailAsync(request.Email))
            .ReturnsAsync((Usuario?)null);
        _mockRepository.Setup(r => r.GetByUsernameAsync(request.Username))
            .ReturnsAsync((Usuario?)null);

        var result = await _authService.RegisterAsync(request);

        result.Success.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Token.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task DeveFalharRegistro_QuandoEmailJaExistir()
    {
        var request = new RegisterRequest(
            Nome: "Test",
            Username: "test",
            Email: "existing@example.com",
            Password: "pass"
        );

        _mockRepository.Setup(r => r.GetByEmailAsync(request.Email))
            .ReturnsAsync(new Usuario { Email = request.Email });

        var result = await _authService.RegisterAsync(request);

        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task DeveFazerLoginComSucesso_QuandoCredenciaisForemValidas()
    {
        var password = "MyPassword123";
        var hashedPassword = BCrypt.Net.BCrypt.HashPassword(password);

        var user = new Usuario
        {
            Id = 1,
            Nome = "Test User",
            Username = "testuser",
            Email = "test@example.com",
            PasswordHash = hashedPassword
        };

        var request = new LoginRequest(
            Username: "testuser",
            Password: password
        );

        _mockRepository.Setup(r => r.GetByUsernameAsync(request.Username))
            .ReturnsAsync(user);

        var result = await _authService.LoginAsync(request);

        result.Success.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Token.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task DeveFalharLogin_QuandoUsuarioNaoExistir()
    {
        var request = new LoginRequest(
            Username: "nonexistent",
            Password: "password"
        );

        _mockRepository.Setup(r => r.GetByUsernameAsync(request.Username))
            .ReturnsAsync((Usuario?)null);

        var result = await _authService.LoginAsync(request);

        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task DeveFalharLogin_QuandoSenhaEstiverIncorreta()
    {
        var correctPassword = "CorrectPassword";
        var hashedPassword = BCrypt.Net.BCrypt.HashPassword(correctPassword);

        var user = new Usuario
        {
            Id = 1,
            Username = "testuser",
            PasswordHash = hashedPassword
        };

        var request = new LoginRequest(
            Username: "testuser",
            Password: "WrongPassword"
        );

        _mockRepository.Setup(r => r.GetByUsernameAsync(request.Username))
            .ReturnsAsync(user);

        var result = await _authService.LoginAsync(request);

        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().NotBeNullOrEmpty();
    }
}
