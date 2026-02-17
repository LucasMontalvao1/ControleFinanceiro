using System.Net;
using System.Net.Http.Json;
using ControleFinanceiro.Application.DTOs;
using ControleFinanceiro.Application.Services;
using FluentAssertions;
using Moq;
using Xunit;

namespace ControleFinanceiro.Tests.Integration.API;

public class DashboardControllerTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public DashboardControllerTests(CustomWebApplicationFactory factory)
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
    public async Task DeveRetornarResumoDashboard_ComSucesso()
    {
        AuthenticateClient();
        
        var dashboardDto = new DashboardSummaryResponse(
            15000, // Total Entradas
            5000,  // Total Saidas
            10000, // Saldo
            new List<CategoriaGraficoResponse> { new("Salário", 15000) },
            new List<EvolucaoDiariaResponse>(),
            new List<EvolucaoMensalGraficoResponse>(),
            new List<LancamentoResponse>()
        );

        _factory.LancamentoServiceMock.Setup(s => s.GetDashboardSummaryAsync(It.IsAny<int>(), It.IsAny<DateTime?>(), It.IsAny<DateTime?>()))
            .ReturnsAsync(Result<DashboardSummaryResponse>.Ok(dashboardDto));

        var response = await _client.GetAsync("/api/dashboard/summary");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadFromJsonAsync<DashboardSummaryResponse>();
        content.Should().NotBeNull();
        content!.TotalEntradas.Should().Be(15000);
        content.Saldo.Should().Be(10000);
    }
}
