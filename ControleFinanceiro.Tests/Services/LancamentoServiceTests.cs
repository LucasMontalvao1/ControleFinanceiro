using ControleFinanceiro.Application.DTOs;
using ControleFinanceiro.Application.Services;
using ControleFinanceiro.Domain.Entities;
using ControleFinanceiro.Domain.Interfaces;
using FluentAssertions;
using Moq;
using Xunit;

namespace ControleFinanceiro.Tests.Services;

public class LancamentoServiceTests
{
    private readonly Mock<ILancamentoRepository> _mockLancamentoRepo;
    private readonly Mock<ICategoriaRepository> _mockCategoriaRepo;
    private readonly LancamentoService _service;

    public LancamentoServiceTests()
    {
        _mockLancamentoRepo = new Mock<ILancamentoRepository>();
        _mockCategoriaRepo = new Mock<ICategoriaRepository>();
        _service = new LancamentoService(_mockLancamentoRepo.Object, _mockCategoriaRepo.Object);
    }

    [Fact]
    public async Task DeveRetornarTodosLancamentos_DoPeriodo()
    {
        var usuarioId = 1;
        var start = new DateTime(2024, 1, 1);
        var end = new DateTime(2024, 1, 31);

        var lancamentos = new List<Lancamento>
        {
            new() { Id = 1, Descricao = "Salário", Valor = 5000, Tipo = "Receita", CategoriaId = 1, UsuarioId = usuarioId, Data = new DateTime(2024, 1, 5) },
            new() { Id = 2, Descricao = "Mercado", Valor = 300, Tipo = "Despesa", CategoriaId = 2, UsuarioId = usuarioId, Data = new DateTime(2024, 1, 10) }
        };

        var categorias = new List<Categoria>
        {
            new() { Id = 1, Nome = "Salário", Tipo = "Receita", UsuarioId = usuarioId },
            new() { Id = 2, Nome = "Alimentação", Tipo = "Despesa", UsuarioId = usuarioId }
        };

        _mockLancamentoRepo.Setup(r => r.GetAllByUsuarioIdAsync(usuarioId, start, end))
            .ReturnsAsync(lancamentos);
        _mockCategoriaRepo.Setup(r => r.GetAllByUsuarioIdAsync(usuarioId))
            .ReturnsAsync(categorias);

        var result = await _service.GetAllAsync(usuarioId, start, end);

        result.Success.Should().BeTrue();
        result.Value.Should().HaveCount(2);
        result.Value.Should().Contain(l => l.Descricao == "Salário");
    }

    [Fact]
    public async Task DeveCriarLancamento_ComSucesso()
    {
        var request = new LancamentoRequest(
            Descricao: "Compra supermercado",
            Valor: 250.50m,
            Data: DateTime.Today,
            Tipo: "Despesa",
            CategoriaId: 1
        );
        var usuarioId = 1;

        _mockLancamentoRepo.Setup(r => r.AddAsync(It.IsAny<Lancamento>()))
            .ReturnsAsync(1);
        _mockCategoriaRepo.Setup(r => r.GetByIdAsync(1, usuarioId))
            .ReturnsAsync(new Categoria { Id = 1, Nome = "Alimentação" });

        var result = await _service.CreateAsync(request, usuarioId);

        result.Success.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Descricao.Should().Be("Compra supermercado");

        _mockLancamentoRepo.Verify(r => r.AddAsync(It.Is<Lancamento>(l =>
            l.Descricao == request.Descricao &&
            l.Valor == request.Valor &&
            l.UsuarioId == usuarioId
        )), Times.Once);
    }

    [Fact]
    public async Task DeveRetornarLancamento_PorId()
    {
        var lancamentoId = 1;
        var usuarioId = 1;

        var lancamento = new Lancamento
        {
            Id = lancamentoId,
            Descricao = "Pagamento conta",
            Valor = 150,
            Tipo = "Despesa",
            CategoriaId = 1,
            UsuarioId = usuarioId,
            Data = DateTime.Today
        };

        var categoria = new Categoria { Id = 1, Nome = "Contas", UsuarioId = usuarioId };

        _mockLancamentoRepo.Setup(r => r.GetByIdAsync(lancamentoId, usuarioId))
            .ReturnsAsync(lancamento);
        _mockCategoriaRepo.Setup(r => r.GetByIdAsync(1, usuarioId))
            .ReturnsAsync(categoria);

        var result = await _service.GetByIdAsync(lancamentoId, usuarioId);

        result.Success.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Descricao.Should().Be("Pagamento conta");
    }

    [Fact]
    public async Task DeveExcluirLancamento_ComSucesso()
    {
        var lancamentoId = 1;
        var usuarioId = 1;

        var lancamento = new Lancamento
        {
            Id = lancamentoId,
            Descricao = "Test",
            UsuarioId = usuarioId
        };

        _mockLancamentoRepo.Setup(r => r.GetByIdAsync(lancamentoId, usuarioId))
            .ReturnsAsync(lancamento);
        _mockLancamentoRepo.Setup(r => r.DeleteAsync(lancamentoId, usuarioId))
            .Returns(Task.CompletedTask);

        var result = await _service.DeleteAsync(lancamentoId, usuarioId);

        result.Success.Should().BeTrue();
        _mockLancamentoRepo.Verify(r => r.DeleteAsync(lancamentoId, usuarioId), Times.Once);
    }

    [Fact]
    public async Task DeveFalharExclusao_QuandoLancamentoNaoExistir()
    {
        var lancamentoId = 999;
        var usuarioId = 1;

        _mockLancamentoRepo.Setup(r => r.GetByIdAsync(lancamentoId, usuarioId))
            .ReturnsAsync((Lancamento?)null);

        var result = await _service.DeleteAsync(lancamentoId, usuarioId);

        result.Success.Should().BeFalse();
        _mockLancamentoRepo.Verify(r => r.DeleteAsync(It.IsAny<int>(), It.IsAny<int>()), Times.Never);
    }
}
