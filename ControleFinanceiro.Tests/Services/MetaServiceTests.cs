using ControleFinanceiro.Application.DTOs;
using ControleFinanceiro.Application.Services;
using ControleFinanceiro.Domain.Entities;
using ControleFinanceiro.Domain.Interfaces;
using FluentAssertions;
using Moq;
using Xunit;

namespace ControleFinanceiro.Tests.Services;

public class MetaServiceTests
{
    private readonly Mock<IMetaRepository> _mockMetaRepo;
    private readonly Mock<ILancamentoRepository> _mockLancamentoRepo;
    private readonly Mock<ICategoriaRepository> _mockCategoriaRepo;
    private readonly MetaService _service;

    public MetaServiceTests()
    {
        _mockMetaRepo = new Mock<IMetaRepository>();
        _mockLancamentoRepo = new Mock<ILancamentoRepository>();
        _mockCategoriaRepo = new Mock<ICategoriaRepository>();
        _service = new MetaService(_mockMetaRepo.Object, _mockLancamentoRepo.Object, _mockCategoriaRepo.Object);
    }

    [Fact]
    public async Task DeveRetornarMetas_ComValorRealizado()
    {
        var usuarioId = 1;
        var mes = 1;
        var ano = 2024;

        var metas = new List<Meta>
        {
            new() { Id = 1, CategoriaId = 1, ValorLimite = 500, Mes = mes, Ano = ano, UsuarioId = usuarioId }
        };

        var categorias = new List<Categoria>
        {
            new() { Id = 1, Nome = "Alimentação", Tipo = "Despesa", UsuarioId = usuarioId }
        };

        var lancamentos = new List<Lancamento>
        {
            new() { Id = 1, CategoriaId = 1, Valor = 200, Tipo = "Saida", UsuarioId = usuarioId, Data = new DateTime(2024, 1, 15) }
        };

        _mockMetaRepo.Setup(r => r.GetAllAsync(usuarioId, mes, ano))
            .ReturnsAsync(metas);
        _mockCategoriaRepo.Setup(r => r.GetAllByUsuarioIdAsync(usuarioId))
            .ReturnsAsync(categorias);
        _mockLancamentoRepo.Setup(r => r.GetAllByUsuarioIdAsync(usuarioId, It.IsAny<DateTime>(), It.IsAny<DateTime>()))
            .ReturnsAsync(lancamentos);

        var result = await _service.GetAllAsync(usuarioId, mes, ano);

        result.Success.Should().BeTrue();
        result.Value.Should().HaveCount(1);
        result.Value.First().ValorLimite.Should().Be(500);
        result.Value.First().ValorRealizado.Should().Be(200);
    }

    [Fact]
    public async Task DeveCriarNovaMeta_QuandoNaoExistir()
    {
        var usuarioId = 1;
        var request = new MetaRequest(
            CategoriaId: 1,
            ValorLimite: 600,
            Mes: 2,
            Ano: 2024
        );

        _mockMetaRepo.Setup(r => r.GetAllAsync(usuarioId, request.Mes, request.Ano))
            .ReturnsAsync(new List<Meta>());
        _mockMetaRepo.Setup(r => r.CreateAsync(It.IsAny<Meta>()))
            .ReturnsAsync(1);

        var result = await _service.CreateOrUpdateAsync(usuarioId, request);

        result.Success.Should().BeTrue();
        result.Value.Should().BeGreaterThan(0);

        _mockMetaRepo.Verify(r => r.CreateAsync(It.Is<Meta>(m =>
            m.CategoriaId == request.CategoriaId &&
            m.ValorLimite == request.ValorLimite &&
            m.Mes == request.Mes &&
            m.Ano == request.Ano
        )), Times.Once);
    }

    [Fact]
    public async Task DeveAtualizarMeta_QuandoJaExistir()
    {
        var usuarioId = 1;
        var request = new MetaRequest(
            CategoriaId: 1,
            ValorLimite: 700,
            Mes: 2,
            Ano: 2024
        );

        var metaExistente = new Meta
        {
            Id = 1,
            CategoriaId = 1,
            ValorLimite = 500,
            Mes = 2,
            Ano = 2024,
            UsuarioId = usuarioId
        };

        _mockMetaRepo.Setup(r => r.GetAllAsync(usuarioId, request.Mes, request.Ano))
            .ReturnsAsync(new List<Meta> { metaExistente });
        _mockMetaRepo.Setup(r => r.UpdateAsync(It.IsAny<Meta>()))
            .Returns(Task.CompletedTask);

        var result = await _service.CreateOrUpdateAsync(usuarioId, request);

        result.Success.Should().BeTrue();
        result.Value.Should().Be(1);

        _mockMetaRepo.Verify(r => r.UpdateAsync(It.Is<Meta>(m =>
            m.Id == 1 &&
            m.ValorLimite == 700
        )), Times.Once);
    }

    [Fact]
    public async Task DeveExcluirMeta_ComSucesso()
    {
        var metaId = 1;
        var usuarioId = 1;

        _mockMetaRepo.Setup(r => r.DeleteAsync(metaId, usuarioId))
            .Returns(Task.CompletedTask);

        var result = await _service.DeleteAsync(metaId, usuarioId);

        result.Success.Should().BeTrue();
        result.Value.Should().BeTrue();
        _mockMetaRepo.Verify(r => r.DeleteAsync(metaId, usuarioId), Times.Once);
    }
}
