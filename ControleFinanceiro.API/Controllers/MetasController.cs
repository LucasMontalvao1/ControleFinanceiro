using System.Security.Claims;
using ControleFinanceiro.Application.DTOs;
using ControleFinanceiro.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ControleFinanceiro.API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class MetasController : ControllerBase
{
    private readonly IMetaService _metaService;

    public MetasController(IMetaService metaService)
    {
        _metaService = metaService;
    }

    private int GetUsuarioId() => int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

    [HttpGet("{mes}/{ano}")]
    public async Task<IActionResult> GetAll(int mes, int ano)
    {
        var result = await _metaService.GetAllAsync(GetUsuarioId(), mes, ano);
        return result.Success ? Ok(result.Value) : BadRequest(result.ErrorMessage);
    }

    [HttpPost]
    public async Task<IActionResult> CreateOrUpdate(MetaRequest request)
    {
        var result = await _metaService.CreateOrUpdateAsync(GetUsuarioId(), request);
        return result.Success ? Ok(result.Value) : BadRequest(result.ErrorMessage);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _metaService.DeleteAsync(id, GetUsuarioId());
        return result.Success ? Ok(result.Value) : BadRequest(result.ErrorMessage);
    }
}
