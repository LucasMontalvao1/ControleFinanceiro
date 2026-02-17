using System.Net;
using System.Net.Http.Json;
using ControleFinanceiro.Application.DTOs;
using ControleFinanceiro.Application.Services;
using FluentAssertions;
using Moq;
using Xunit;

namespace ControleFinanceiro.Tests.Integration.API;

public class LancamentosControllerTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public LancamentosControllerTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = _factory.CreateClient();
    }

    private void AuthenticateClient()
    {
        var token = _factory.GenerateJwtToken(1, "test@example.com");
        _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
    }

    [Fact]
    public async Task DeveCriarLancamento_ComSucesso()
    {
        AuthenticateClient();
        // Request: Descricao, Valor, Data, Tipo, CategoriaId
        var request = new LancamentoRequest("Salário", 5000, DateTime.Now, "Entrada", 1);
        
        // Response: Id, Descricao, Valor, Data, Tipo, CategoriaId, CategoriaNome
        var responseDto = new LancamentoResponse(1, "Salário", 5000, DateTime.Now, "Receita", 1, "Receita");

        _factory.LancamentoServiceMock.Setup(s => s.CreateAsync(It.IsAny<LancamentoRequest>(), It.IsAny<int>()))
            .ReturnsAsync(Result<LancamentoResponse>.Ok(responseDto));

        var response = await _client.PostAsJsonAsync("/api/lancamentos", request);

        // Controller returns CreatedAtAction (201)
        response.StatusCode.Should().Be(HttpStatusCode.Created, because: await response.Content.ReadAsStringAsync());
        var content = await response.Content.ReadFromJsonAsync<LancamentoResponse>();
        content.Should().NotBeNull();
        content!.Descricao.Should().Be("Salário");
    }

    [Fact]
    public async Task DeveListarLancamentos_ComSucesso()
    {
        AuthenticateClient();
        var responseList = new List<LancamentoResponse>
        {
            new(1, "Teste 1", 100, DateTime.Now, "Despesa", 1, "Despesa"),
            new(2, "Teste 2", 200, DateTime.Now, "Receita", 1, "Receita")
        };

        _factory.LancamentoServiceMock.Setup(s => s.GetAllAsync(It.IsAny<int>(), It.IsAny<DateTime?>(), It.IsAny<DateTime?>()))
            .ReturnsAsync(Result<IEnumerable<LancamentoResponse>>.Ok(responseList));

        var response = await _client.GetAsync("/api/lancamentos");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadFromJsonAsync<IEnumerable<LancamentoResponse>>();
        content.Should().HaveCount(2);
    }

    [Fact]
    public async Task DeveExcluirLancamento_ComSucesso()
    {
        AuthenticateClient();
        _factory.LancamentoServiceMock.Setup(s => s.DeleteAsync(It.IsAny<int>(), It.IsAny<int>()))
            .ReturnsAsync(Result.Ok());

        var response = await _client.DeleteAsync("/api/lancamentos/1");

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }
}
