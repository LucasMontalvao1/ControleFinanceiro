using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ControleFinanceiro.Application.DTOs;
using ControleFinanceiro.Application.Services;
using System.Security.Claims;

namespace ControleFinanceiro.API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class CategoriasController : ControllerBase
{
    private readonly ICategoriaService _categoriaService;

    public CategoriasController(ICategoriaService categoriaService)
    {
        _categoriaService = categoriaService;
    }

    private int GetUsuarioId()
    {
        var claim = User.FindFirst(ClaimTypes.NameIdentifier);
        return int.Parse(claim?.Value ?? "0");
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var result = await _categoriaService.GetAllAsync(GetUsuarioId());
        return Ok(result.Value);
    }

    [HttpPost]
    public async Task<IActionResult> Create(CategoriaRequest request)
    {
        var result = await _categoriaService.CreateAsync(request, GetUsuarioId());
        return StatusCode(201, result.Value);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, CategoriaRequest request)
    {
        var result = await _categoriaService.UpdateAsync(id, request, GetUsuarioId());
        if (!result.Success) return BadRequest(new { message = result.ErrorMessage });
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _categoriaService.DeleteAsync(id, GetUsuarioId());
        if (!result.Success) return BadRequest(new { message = result.ErrorMessage });
        return NoContent();
    }
}
