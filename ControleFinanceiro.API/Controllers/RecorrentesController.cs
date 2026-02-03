using System.Security.Claims;
using ControleFinanceiro.Application.DTOs;
using ControleFinanceiro.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ControleFinanceiro.API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class RecorrentesController : ControllerBase
{
    private readonly IRecorrenteService _recorrenteService;

    public RecorrentesController(IRecorrenteService recorrenteService)
    {
        _recorrenteService = recorrenteService;
    }

    private int GetUsuarioId() => int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var result = await _recorrenteService.GetAllAsync(GetUsuarioId());
        return result.Success ? Ok(result.Value) : BadRequest(result.ErrorMessage);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _recorrenteService.GetByIdAsync(id, GetUsuarioId());
        return result.Success ? Ok(result.Value) : NotFound(result.ErrorMessage);
    }

    [HttpPost]
    public async Task<IActionResult> Create(RecorrenteRequest request)
    {
        var result = await _recorrenteService.CreateAsync(GetUsuarioId(), request);
        return result.Success ? CreatedAtAction(nameof(GetById), new { id = result.Value }, result.Value) : BadRequest(result.ErrorMessage);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, RecorrenteRequest request)
    {
        var result = await _recorrenteService.UpdateAsync(id, GetUsuarioId(), request);
        return result.Success ? Ok(result.Value) : BadRequest(result.ErrorMessage);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _recorrenteService.DeleteAsync(id, GetUsuarioId());
        return result.Success ? Ok(result.Value) : BadRequest(result.ErrorMessage);
    }

    [HttpPost("apply/{mes}/{ano}")]
    public async Task<IActionResult> ApplyToMonth(int mes, int ano)
    {
        var result = await _recorrenteService.ApplyToMonthAsync(GetUsuarioId(), mes, ano);
        return result.Success ? Ok(result.Value) : BadRequest(result.ErrorMessage);
    }
}
