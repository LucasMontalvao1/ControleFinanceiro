using System.Net;
using System.Net.Http.Json;
using ControleFinanceiro.Application.DTOs;
using ControleFinanceiro.Application.Services;
using FluentAssertions;
using Moq;
using Xunit;

namespace ControleFinanceiro.Tests.Integration.API;

public class RecorrentesControllerTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public RecorrentesControllerTests(CustomWebApplicationFactory factory)
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
    public async Task DeveCriarRecorrente_ComSucesso()
    {
        AuthenticateClient();
        var request = new RecorrenteRequest(1, "Aluguel", 1500, 5, "Despesa", true);
        
        _factory.RecorrenteServiceMock.Setup(s => s.CreateAsync(It.IsAny<int>(), It.IsAny<RecorrenteRequest>()))
            .ReturnsAsync(Result<int>.Ok(1));

        var response = await _client.PostAsJsonAsync("/api/recorrentes", request);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var content = await response.Content.ReadFromJsonAsync<int>();
        content.Should().Be(1);
    }

    [Fact]
    public async Task DeveListarRecorrentes_ComSucesso()
    {
        AuthenticateClient();
        var responseList = new List<RecorrenteResponse>
        {
            new(1, 1, "Moradia", "Aluguel", 1500, 5, "Despesa", true),
            new(2, 2, "Lazer", "Netflix", 50, 10, "Despesa", true)
        };

        _factory.RecorrenteServiceMock.Setup(s => s.GetAllAsync(It.IsAny<int>()))
            .ReturnsAsync(Result<IEnumerable<RecorrenteResponse>>.Ok(responseList));

        var response = await _client.GetAsync("/api/recorrentes");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadFromJsonAsync<IEnumerable<RecorrenteResponse>>();
        content.Should().HaveCount(2);
    }

    [Fact]
    public async Task DeveExcluirRecorrente_ComSucesso()
    {
        AuthenticateClient();
        _factory.RecorrenteServiceMock.Setup(s => s.DeleteAsync(It.IsAny<int>(), It.IsAny<int>()))
            .ReturnsAsync(Result<bool>.Ok(true));

        var response = await _client.DeleteAsync("/api/recorrentes/1");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
