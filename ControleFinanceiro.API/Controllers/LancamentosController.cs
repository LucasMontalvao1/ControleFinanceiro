using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ControleFinanceiro.Application.DTOs;
using ControleFinanceiro.Application.Services;
using System.Security.Claims;

namespace ControleFinanceiro.API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class LancamentosController : ControllerBase
{
    private readonly ILancamentoService _lancamentoService;

    public LancamentosController(ILancamentoService lancamentoService)
    {
        _lancamentoService = lancamentoService;
    }

    private int GetUsuarioId()
    {
        var claim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (claim == null || string.IsNullOrEmpty(claim.Value)) return 0;
        return int.Parse(claim.Value);
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] DateTime? start, [FromQuery] DateTime? end)
    {
        var result = await _lancamentoService.GetAllAsync(GetUsuarioId(), start, end);
        return Ok(result.Value);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _lancamentoService.GetByIdAsync(id, GetUsuarioId());
        if (!result.Success) return NotFound(new { message = result.ErrorMessage });
        return Ok(result.Value);
    }

    [HttpPost]
    public async Task<IActionResult> Create(LancamentoRequest request)
    {
        var result = await _lancamentoService.CreateAsync(request, GetUsuarioId());
        if (!result.Success) return BadRequest(new { message = result.ErrorMessage });
        return CreatedAtAction(nameof(GetById), new { id = result.Value.Id }, result.Value);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, LancamentoRequest request)
    {
        var result = await _lancamentoService.UpdateAsync(id, request, GetUsuarioId());
        if (!result.Success) return BadRequest(new { message = result.ErrorMessage });
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _lancamentoService.DeleteAsync(id, GetUsuarioId());
        if (!result.Success) return BadRequest(new { message = result.ErrorMessage });
        return NoContent();
    }
}
