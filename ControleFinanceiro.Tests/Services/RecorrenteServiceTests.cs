using ControleFinanceiro.Application.DTOs;
using ControleFinanceiro.Application.Services;
using ControleFinanceiro.Domain.Entities;
using ControleFinanceiro.Domain.Interfaces;
using FluentAssertions;
using Moq;
using Xunit;

namespace ControleFinanceiro.Tests.Services;

public class RecorrenteServiceTests
{
    private readonly Mock<IRecorrenteRepository> _mockRecorrenteRepo;
    private readonly Mock<ILancamentoRepository> _mockLancamentoRepo;
    private readonly RecorrenteService _service;

    public RecorrenteServiceTests()
    {
        _mockRecorrenteRepo = new Mock<IRecorrenteRepository>();
        _mockLancamentoRepo = new Mock<ILancamentoRepository>();
        _service = new RecorrenteService(_mockRecorrenteRepo.Object, _mockLancamentoRepo.Object);
    }

    [Fact]
    public async Task DeveRetornarTodosRecorrentes_DoUsuario()
    {
        var usuarioId = 1;
        var recorrentes = new List<Recorrente>
        {
            new() { Id = 1, Descricao = "Aluguel", Valor = 1200, DiaVencimento = 5, Tipo = "Despesa", Ativo = true, UsuarioId = usuarioId },
            new() { Id = 2, Descricao = "Salário", Valor = 5000, DiaVencimento = 1, Tipo = "Receita", Ativo = true, UsuarioId = usuarioId }
        };

        _mockRecorrenteRepo.Setup(r => r.GetAllAsync(usuarioId))
            .ReturnsAsync(recorrentes);

        var result = await _service.GetAllAsync(usuarioId);

        result.Success.Should().BeTrue();
        result.Value.Should().HaveCount(2);
        result.Value.Should().Contain(r => r.Descricao == "Aluguel");
    }

    [Fact]
    public async Task DeveCriarRecorrente_ComSucesso()
    {
        var usuarioId = 1;
        var request = new RecorrenteRequest(
            CategoriaId: 1,
            Descricao: "Netflix",
            Valor: 45.90m,
            DiaVencimento: 15,
            Tipo: "Despesa",
            Ativo: true
        );

        _mockRecorrenteRepo.Setup(r => r.CreateAsync(It.IsAny<Recorrente>()))
            .ReturnsAsync(1);

        var result = await _service.CreateAsync(usuarioId, request);

        result.Success.Should().BeTrue();
        result.Value.Should().BeGreaterThan(0);

        _mockRecorrenteRepo.Verify(r => r.CreateAsync(It.Is<Recorrente>(rec =>
            rec.Descricao == request.Descricao &&
            rec.Valor == request.Valor &&
            rec.DiaVencimento == request.DiaVencimento
        )), Times.Once);
    }

    [Fact]
    public async Task DeveAtualizarRecorrente_QuandoExistir()
    {
        var recorrenteId = 1;
        var usuarioId = 1;
        var request = new RecorrenteRequest(
            CategoriaId: 1,
            Descricao: "Netflix Premium",
            Valor: 55.90m,
            DiaVencimento: 15,
            Tipo: "Despesa",
            Ativo: true
        );

        var recorrenteExistente = new Recorrente
        {
            Id = recorrenteId,
            Descricao = "Netflix",
            Valor = 45.90m,
            UsuarioId = usuarioId
        };

        _mockRecorrenteRepo.Setup(r => r.GetByIdAsync(recorrenteId, usuarioId))
            .ReturnsAsync(recorrenteExistente);
        _mockRecorrenteRepo.Setup(r => r.UpdateAsync(It.IsAny<Recorrente>()))
            .Returns(Task.CompletedTask);

        var result = await _service.UpdateAsync(recorrenteId, usuarioId, request);

        result.Success.Should().BeTrue();
        _mockRecorrenteRepo.Verify(r => r.UpdateAsync(It.Is<Recorrente>(rec =>
            rec.Descricao == "Netflix Premium" &&
            rec.Valor == 55.90m
        )), Times.Once);
    }

    [Fact]
    public async Task DeveExcluirRecorrente_ComSucesso()
    {
        var recorrenteId = 1;
        var usuarioId = 1;

        _mockRecorrenteRepo.Setup(r => r.DeleteAsync(recorrenteId, usuarioId))
            .Returns(Task.CompletedTask);

        var result = await _service.DeleteAsync(recorrenteId, usuarioId);

        result.Success.Should().BeTrue();
        _mockRecorrenteRepo.Verify(r => r.DeleteAsync(recorrenteId, usuarioId), Times.Once);
    }

    [Fact]
    public async Task DeveAplicarRecorrentes_AoMes()
    {
        var usuarioId = 1;
        var mes = 2;
        var ano = 2024;

        var recorrentes = new List<Recorrente>
        {
            new() { Id = 1, Descricao = "Aluguel", Valor = 1200, DiaVencimento = 5, Tipo = "Despesa", Ativo = true, CategoriaId = 1, UsuarioId = usuarioId },
            new() { Id = 2, Descricao = "Internet", Valor = 100, DiaVencimento = 10, Tipo = "Despesa", Ativo = true, CategoriaId = 2, UsuarioId = usuarioId }
        };

        _mockRecorrenteRepo.Setup(r => r.GetAllAsync(usuarioId))
            .ReturnsAsync(recorrentes);
        _mockLancamentoRepo.Setup(r => r.GetAllByUsuarioIdAsync(usuarioId, It.IsAny<DateTime>(), It.IsAny<DateTime>()))
            .ReturnsAsync(new List<Lancamento>());
        _mockLancamentoRepo.Setup(r => r.AddAsync(It.IsAny<Lancamento>()))
            .ReturnsAsync(1);

        var result = await _service.ApplyToMonthAsync(usuarioId, mes, ano);

        result.Success.Should().BeTrue();
        result.Value.Should().Be(2);
        _mockLancamentoRepo.Verify(r => r.AddAsync(It.IsAny<Lancamento>()), Times.Exactly(2));
    }

    [Fact]
    public async Task NaoDeveAplicarRecorrentes_JaAplicados()
    {
        var usuarioId = 1;
        var mes = 2;
        var ano = 2024;

        var recorrentes = new List<Recorrente>
        {
            new() { Id = 1, Descricao = "Aluguel", Valor = 1200, DiaVencimento = 5, Tipo = "Despesa", Ativo = true, CategoriaId = 1, UsuarioId = usuarioId }
        };

        var lancamentosExistentes = new List<Lancamento>
        {
            new() { Id = 1, RecorrenteId = 1, Descricao = "Aluguel", Valor = 1200, UsuarioId = usuarioId }
        };

        _mockRecorrenteRepo.Setup(r => r.GetAllAsync(usuarioId))
            .ReturnsAsync(recorrentes);
        _mockLancamentoRepo.Setup(r => r.GetAllByUsuarioIdAsync(usuarioId, It.IsAny<DateTime>(), It.IsAny<DateTime>()))
            .ReturnsAsync(lancamentosExistentes);

        var result = await _service.ApplyToMonthAsync(usuarioId, mes, ano);

        result.Success.Should().BeTrue();
        result.Value.Should().Be(0);
        _mockLancamentoRepo.Verify(r => r.AddAsync(It.IsAny<Lancamento>()), Times.Never);
    }
}
