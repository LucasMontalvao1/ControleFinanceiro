using ControleFinanceiro.Application.DTOs;
using ControleFinanceiro.Application.Services;
using ControleFinanceiro.Domain.Entities;
using ControleFinanceiro.Domain.Interfaces;
using FluentAssertions;
using Moq;
using Xunit;

namespace ControleFinanceiro.Tests.Services;

public class CategoriaServiceTests
{
    private readonly Mock<ICategoriaRepository> _mockRepository;
    private readonly CategoriaService _service;

    public CategoriaServiceTests()
    {
        _mockRepository = new Mock<ICategoriaRepository>();
        _service = new CategoriaService(_mockRepository.Object);
    }

    [Fact]
    public async Task DeveRetornarTodasCategorias_DoUsuario()
    {
        var usuarioId = 1;
        var categorias = new List<Categoria>
        {
            new() { Id = 1, Nome = "Alimentação", Tipo = "Despesa", UsuarioId = usuarioId, IsDefault = false },
            new() { Id = 2, Nome = "Salário", Tipo = "Receita", UsuarioId = usuarioId, IsDefault = false }
        };

        _mockRepository.Setup(r => r.GetAllByUsuarioIdAsync(usuarioId))
            .ReturnsAsync(categorias);

        var result = await _service.GetAllAsync(usuarioId);

        result.Success.Should().BeTrue();
        result.Value.Should().HaveCount(2);
        result.Value.Should().Contain(c => c.Nome == "Alimentação");
    }

    [Fact]
    public async Task DeveCriarCategoria_ComSucesso()
    {
        var request = new CategoriaRequest(
            Nome: "Nova Categoria",
            Tipo: "Despesa"
        );
        var usuarioId = 1;

        _mockRepository.Setup(r => r.AddAsync(It.IsAny<Categoria>()))
            .ReturnsAsync(1);

        var result = await _service.CreateAsync(request, usuarioId);

        result.Success.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Nome.Should().Be("Nova Categoria");

        _mockRepository.Verify(r => r.AddAsync(It.Is<Categoria>(c =>
            c.Nome == request.Nome &&
            c.Tipo == request.Tipo &&
            c.UsuarioId == usuarioId
        )), Times.Once);
    }

    [Fact]
    public async Task DeveExcluirCategoria_QuandoNaoForPadrao()
    {
        var categoriaId = 1;
        var usuarioId = 1;

        var categoria = new Categoria
        {
            Id = categoriaId,
            Nome = "Test",
            Tipo = "Despesa",
            UsuarioId = usuarioId,
            IsDefault = false
        };

        _mockRepository.Setup(r => r.GetByIdAsync(categoriaId, usuarioId))
            .ReturnsAsync(categoria);
        _mockRepository.Setup(r => r.DeleteAsync(categoriaId, usuarioId))
            .Returns(Task.CompletedTask);

        var result = await _service.DeleteAsync(categoriaId, usuarioId);

        result.Success.Should().BeTrue();
        _mockRepository.Verify(r => r.DeleteAsync(categoriaId, usuarioId), Times.Once);
    }

    [Fact]
    public async Task DeveFalharExclusao_QuandoCategoriaForPadrao()
    {
        var categoriaId = 1;
        var usuarioId = 1;

        var categoria = new Categoria
        {
            Id = categoriaId,
            Nome = "Default Category",
            Tipo = "Despesa",
            UsuarioId = usuarioId,
            IsDefault = true
        };

        _mockRepository.Setup(r => r.GetByIdAsync(categoriaId, usuarioId))
            .ReturnsAsync(categoria);

        var result = await _service.DeleteAsync(categoriaId, usuarioId);

        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Contain("padrão");
        _mockRepository.Verify(r => r.DeleteAsync(It.IsAny<int>(), It.IsAny<int>()), Times.Never);
    }
}
