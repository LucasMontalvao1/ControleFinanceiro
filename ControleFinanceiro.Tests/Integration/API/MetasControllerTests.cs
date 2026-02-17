using System.Net;
using System.Net.Http.Json;
using ControleFinanceiro.Application.DTOs;
using ControleFinanceiro.Application.Services;
using FluentAssertions;
using Moq;
using Xunit;

namespace ControleFinanceiro.Tests.Integration.API;

public class MetasControllerTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public MetasControllerTests(CustomWebApplicationFactory factory)
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
    public async Task DeveCriarMeta_ComSucesso()
    {
        AuthenticateClient();
        var request = new MetaRequest(1, 1000, 2, 2026);

        _factory.MetaServiceMock.Setup(s => s.CreateOrUpdateAsync(It.IsAny<int>(), It.IsAny<MetaRequest>()))
            .ReturnsAsync(Result<int>.Ok(1));

        var response = await _client.PostAsJsonAsync("/api/metas", request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadFromJsonAsync<int>();
        content.Should().Be(1);
    }

    [Fact]
    public async Task DeveListarMetas_ComSucesso()
    {
        AuthenticateClient();
        var responseList = new List<MetaResponse>
        {
            new(1, 1, "Alimentação", 1000, 200, 2, 2026),
            new(2, 2, "Transporte", 500, 100, 2, 2026)
        };

        _factory.MetaServiceMock.Setup(s => s.GetAllAsync(It.IsAny<int>(), 2, 2026))
            .ReturnsAsync(Result<IEnumerable<MetaResponse>>.Ok(responseList));

        var response = await _client.GetAsync("/api/metas/2/2026");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadFromJsonAsync<IEnumerable<MetaResponse>>();
        content.Should().HaveCount(2);
    }

    [Fact]
    public async Task DeveExcluirMeta_ComSucesso()
    {
        AuthenticateClient();
        _factory.MetaServiceMock.Setup(s => s.DeleteAsync(It.IsAny<int>(), It.IsAny<int>()))
            .ReturnsAsync(Result<bool>.Ok(true));

        var response = await _client.DeleteAsync("/api/metas/1");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
