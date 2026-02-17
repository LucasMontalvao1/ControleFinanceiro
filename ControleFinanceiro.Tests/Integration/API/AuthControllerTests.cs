using System.Net;
using System.Net.Http.Json;
using ControleFinanceiro.Application.DTOs;
using ControleFinanceiro.Application.Services;
using FluentAssertions;
using Moq;
using Xunit;

namespace ControleFinanceiro.Tests.Integration.API;

public class AuthControllerTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public AuthControllerTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = _factory.CreateClient();
    }

    [Fact]
    public async Task DeveFazerLogin_ComSucesso()
    {
        var request = new RegisterRequest("Test User", "testuser", "test@example.com", "password123");
        var responseDto = new AuthResponse("token_jwt", "Test User", "test@example.com");

        // Setup mock
        _factory.AuthServiceMock.Setup(s => s.RegisterAsync(It.IsAny<RegisterRequest>()))
            .ReturnsAsync(Result<AuthResponse>.Ok(responseDto));

        var response = await _client.PostAsJsonAsync("/api/auth/register", request);

        var contentStr = await response.Content.ReadAsStringAsync();
        response.StatusCode.Should().Be(HttpStatusCode.OK, contentStr);
        
        var content = await response.Content.ReadFromJsonAsync<AuthResponse>();
        content.Should().NotBeNull();
    }

    [Fact]
    public async Task DeveRegistrarUsuario_ComSucesso()
    {
        var request = new RegisterRequest("Test User", "testuser", "test@example.com", "password123");
        var responseDto = new AuthResponse("token_jwt", "Test User", "test@example.com");

        // Setup mock
        _factory.AuthServiceMock.Setup(s => s.RegisterAsync(It.IsAny<RegisterRequest>()))
            .ReturnsAsync(Result<AuthResponse>.Ok(responseDto));

        var response = await _client.PostAsJsonAsync("/api/auth/register", request);

        var contentStr = await response.Content.ReadAsStringAsync();
        response.StatusCode.Should().Be(HttpStatusCode.OK, contentStr);
        
        var content = await response.Content.ReadFromJsonAsync<AuthResponse>();
        content.Should().NotBeNull();
        content!.Token.Should().Be("token_jwt");
    }

    [Fact]
    public async Task DeveFalharRegistro_QuandoEmailJaExiste()
    {
        var request = new RegisterRequest("Test User", "testuser", "existing@example.com", "password123");

        _factory.AuthServiceMock.Setup(s => s.RegisterAsync(It.IsAny<RegisterRequest>()))
            .ReturnsAsync(Result<AuthResponse>.Fail("Email já cadastrado."));

        var response = await _client.PostAsJsonAsync("/api/auth/register", request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}
