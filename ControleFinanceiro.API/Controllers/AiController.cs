using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using ControleFinanceiro.Application.DTOs.Ai;
using ControleFinanceiro.Application.Services;
using ControleFinanceiro.Domain.Interfaces;
using ControleFinanceiro.Domain.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ControleFinanceiro.API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class AiController : ControllerBase
{
    private readonly IAiReceiptService _aiService;
    private readonly ILogger<AiController> _logger;

    public AiController(IAiReceiptService aiService, ILogger<AiController> logger)
    {
        _aiService = aiService;
        _logger = logger;
    }

    [HttpPost("analyze")]
    public async Task<IActionResult> Analyze([FromForm] IFormFile file)
    {
        var correlationId = Guid.NewGuid().ToString();
        var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (!int.TryParse(userIdString, out int userId))
        {
            _logger.LogWarning("{CorrelationId} ID de usuário não encontrado no token", correlationId);
            return Unauthorized();
        }

        if (file == null || file.Length == 0)
        {
            return BadRequest("Arquivo de imagem não fornecido.");
        }

        using var ms = new MemoryStream();
        await file.CopyToAsync(ms);
        var base64Image = Convert.ToBase64String(ms.ToArray());

        _logger.LogInformation("{CorrelationId} Iniciando a análise de IA para o usuário {UserId}", correlationId, userId);

        var result = await _aiService.AnalyzeReceiptAsync(base64Image, userId, correlationId);

        if (result != null)
        {
            return Ok(result);
        }

        _logger.LogError("{CorrelationId} A análise de IA falhou ou retornou nulo.", correlationId);
        return BadRequest("Erro ao processar imagem na IA.");
    }
}
