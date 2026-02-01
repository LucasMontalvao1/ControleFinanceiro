using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ControleFinanceiro.Application.Services;
using System.Security.Claims;

namespace ControleFinanceiro.API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class DashboardController : ControllerBase
{
    private readonly ILancamentoService _lancamentoService;

    public DashboardController(ILancamentoService lancamentoService)
    {
        _lancamentoService = lancamentoService;
    }

    private int GetUsuarioId()
    {
        var claim = User.FindFirst(ClaimTypes.NameIdentifier);
        return int.Parse(claim?.Value ?? "0");
    }

    [HttpGet("summary")]
    public async Task<IActionResult> GetSummary([FromQuery] DateTime? start, [FromQuery] DateTime? end)
    {
        var result = await _lancamentoService.GetDashboardSummaryAsync(GetUsuarioId(), start, end);
        return Ok(result.Value);
    }
}
