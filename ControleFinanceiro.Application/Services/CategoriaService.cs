using ControleFinanceiro.Application.DTOs;
using ControleFinanceiro.Application.Services;
using ControleFinanceiro.Domain.Entities;
using ControleFinanceiro.Domain.Interfaces;

namespace ControleFinanceiro.Application.Services;

public interface ICategoriaService
{
    Task<Result<IEnumerable<CategoriaResponse>>> GetAllAsync(int usuarioId);
    Task<Result<CategoriaResponse>> CreateAsync(CategoriaRequest request, int usuarioId);
    Task<Result> UpdateAsync(int id, CategoriaRequest request, int usuarioId);
    Task<Result> DeleteAsync(int id, int usuarioId);
}

public class CategoriaService : ICategoriaService
{
    private readonly ICategoriaRepository _categoriaRepository;

    public CategoriaService(ICategoriaRepository categoriaRepository)
    {
        _categoriaRepository = categoriaRepository;
    }

    public async Task<Result<IEnumerable<CategoriaResponse>>> GetAllAsync(int usuarioId)
    {
        var categorias = await _categoriaRepository.GetAllByUsuarioIdAsync(usuarioId);
        var response = categorias.Select(c => new CategoriaResponse(c.Id, c.Nome, c.Tipo, c.IsDefault));
        return Result<IEnumerable<CategoriaResponse>>.Ok(response);
    }

    public async Task<Result<CategoriaResponse>> CreateAsync(CategoriaRequest request, int usuarioId)
    {
        var categoria = new Categoria
        {
            Nome = request.Nome,
            Tipo = request.Tipo,
            UsuarioId = usuarioId,
            IsDefault = false
        };

        var id = await _categoriaRepository.AddAsync(categoria);
        return Result<CategoriaResponse>.Ok(new CategoriaResponse(id, categoria.Nome, categoria.Tipo, false));
    }

    public async Task<Result> UpdateAsync(int id, CategoriaRequest request, int usuarioId)
    {
        var categoria = await _categoriaRepository.GetByIdAsync(id, usuarioId);
        if (categoria == null) return Result.Fail("Categoria não encontrada.");
        if (categoria.IsDefault) return Result.Fail("Não é possível editar categorias padrão.");

        categoria.Nome = request.Nome;
        categoria.Tipo = request.Tipo;

        await _categoriaRepository.UpdateAsync(categoria);
        return Result.Ok();
    }

    public async Task<Result> DeleteAsync(int id, int usuarioId)
    {
        var categoria = await _categoriaRepository.GetByIdAsync(id, usuarioId);
        if (categoria == null) return Result.Fail("Categoria não encontrada.");
        if (categoria.IsDefault) return Result.Fail("Não é possível remover categorias padrão.");

        await _categoriaRepository.DeleteAsync(id, usuarioId);
        return Result.Ok();
    }
}
