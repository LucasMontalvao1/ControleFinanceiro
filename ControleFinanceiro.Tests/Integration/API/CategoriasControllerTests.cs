using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using ControleFinanceiro.Application.DTOs;
using ControleFinanceiro.Application.Services;
using FluentAssertions;
using Moq;
using Xunit;

namespace ControleFinanceiro.Tests.Integration.API;

public class CategoriasControllerTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public CategoriasControllerTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = _factory.CreateClient();
    }

    private void AuthenticateClient(int userId = 1)
    {
        var token = _factory.GenerateJwtToken(userId, "test@example.com");
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
    }

    [Fact]
    public async Task DeveListarCategorias_ComSucesso()
    {
        AuthenticateClient();
        var categorias = new List<CategoriaResponse>
        {
            new(1, "Alimentação", "Despesa", false),
            new(2, "Salário", "Receita", false)
        };

        _factory.CategoriaServiceMock.Setup(s => s.GetAllAsync(It.IsAny<int>()))
            .ReturnsAsync(Result<IEnumerable<CategoriaResponse>>.Ok(categorias));

        var response = await _client.GetAsync("/api/categorias");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadFromJsonAsync<IEnumerable<CategoriaResponse>>();
        content.Should().HaveCount(2);
    }

    [Fact]
    public async Task DeveCriarCategoria_ComSucesso()
    {
        AuthenticateClient();
        var request = new CategoriaRequest("Nova Categoria", "Despesa");
        var responseDto = new CategoriaResponse(1, "Nova Categoria", "Despesa", false);

        _factory.CategoriaServiceMock.Setup(s => s.CreateAsync(It.IsAny<CategoriaRequest>(), It.IsAny<int>()))
            .ReturnsAsync(Result<CategoriaResponse>.Ok(responseDto));

        var response = await _client.PostAsJsonAsync("/api/categorias", request);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var content = await response.Content.ReadFromJsonAsync<CategoriaResponse>();
        content.Should().NotBeNull();
        content!.Nome.Should().Be("Nova Categoria");
    }

    [Fact]
    public async Task DeveFalharCriacao_QuandoDadosInvalidos()
    {
        AuthenticateClient();
        var request = new CategoriaRequest("", "Despesa"); // Nome vazio
        
        var response = await _client.PostAsJsonAsync("/api/categorias", request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task DeveExcluirCategoria_ComSucesso()
    {
        AuthenticateClient();
        int categoriaId = 1;

        _factory.CategoriaServiceMock.Setup(s => s.DeleteAsync(categoriaId, It.IsAny<int>()))
            .ReturnsAsync(Result.Ok());

        var response = await _client.DeleteAsync($"/api/categorias/{categoriaId}");

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }
}
