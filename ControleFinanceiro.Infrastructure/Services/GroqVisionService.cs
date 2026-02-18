using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using ControleFinanceiro.Domain.DTOs;
using ControleFinanceiro.Domain.Entities;
using ControleFinanceiro.Domain.Interfaces;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace ControleFinanceiro.Infrastructure.Services;

public class GroqVisionService : IAiReceiptService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly IAiScanHistoryRepository _historyRepository;
    private readonly IMemoryCache _cache;
    private readonly ILogger<GroqVisionService> _logger;

    private const string Model = "meta-llama/llama-4-scout-17b-16e-instruct";
    private const int MaxDailyScans = 25;
    private const int MaxMinuteScans = 5;

    public GroqVisionService(
        HttpClient httpClient,
        IConfiguration configuration,
        IAiScanHistoryRepository historyRepository,
        IMemoryCache cache,
        ILogger<GroqVisionService> logger)
    {
        _httpClient = httpClient;
        _configuration = configuration;
        _historyRepository = historyRepository;
        _cache = cache;
        _logger = logger;
    }

    public async Task<ReceiptAnalysisResponse?> AnalyzeReceiptAsync(string base64Image, int usuarioId, string correlationId)
    {
        // 1. Rate Limiting Check
        if (!CheckRateLimit(usuarioId))
        {
            _logger.LogWarning("Rate limit exceeded for user {UsuarioId} on request {CorrelationId}", usuarioId, correlationId);
            return null;
        }

        var history = new AiScanHistory
        {
            CorrelationId = correlationId,
            UsuarioId = usuarioId,
            Status = "Processing",
            ProcessadoEm = DateTime.UtcNow
        };

        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        try
        {
            if (base64Image.Contains(","))
            {
                base64Image = base64Image.Split(',')[1];
            }
            // 2. Prepare Request
            var apiKey = _configuration["Groq:ApiKey"];
            if (string.IsNullOrEmpty(apiKey))
            {
                _logger.LogError("{CorrelationId} Groq API Key is missing in configuration", correlationId);
                return null;
            }

            _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", apiKey);

            _logger.LogInformation("Analyzing receipt via Groq (Model: {Model}, Correlation: {CorrelationId})", Model, correlationId);

            var payload = new
            {
                model = Model,
                messages = new[]
                {
                    new
                    {
                        role = "user",
                        content = new object[]
                        {
                            new { type = "text", text = GetSystemPrompt() },
                            new { type = "image_url", image_url = new { url = $"data:image/jpeg;base64,{base64Image}" } }
                        }
                    }
                },
                temperature = 0.1,
                max_tokens = 2048,
                top_p = 1,
                stream = false,
                response_format = new { type = "json_object" }
            };

            // 3. Call API
            var response = await _httpClient.PostAsJsonAsync("chat/completions", payload);
            stopwatch.Stop();
            history.LatencyMs = stopwatch.ElapsedMilliseconds;

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                history.Status = "Failed";
                history.ParseError = $"API Error: {response.StatusCode} - {errorContent}";
                await _historyRepository.AddAsync(history);
                
                _logger.LogError("{CorrelationId} Groq API Error: {StatusCode} - {Content}", correlationId, response.StatusCode, errorContent);
                return null;
            }

            // 4. Parse Response
            var jsonResponse = await response.Content.ReadAsStringAsync();
            _logger.LogDebug("{CorrelationId} Raw API response: {Response}", correlationId, jsonResponse);

            var chatCompletion = JsonSerializer.Deserialize<JsonElement>(jsonResponse);
            var content = chatCompletion.GetProperty("choices")[0].GetProperty("message").GetProperty("content").GetString();

            _logger.LogInformation("{CorrelationId} AI Content: {Content}", correlationId, content);
            history.RawJson = content;

            // 5. Sanitize & Deserialize
            var sanitizedJson = SanitizeJson(content!);
            _logger.LogInformation("{CorrelationId} Sanitized JSON: {Json}", correlationId, sanitizedJson);

            var result = JsonSerializer.Deserialize<ReceiptAnalysisResponse>(sanitizedJson, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (result == null)
            {
                 history.Status = "Failed";
                 history.ParseError = "Deserialization returned null";
                 await _historyRepository.AddAsync(history);
                 return null;
            }

            // 6. Post-AI Validation (Simplified)
            if (result.TotalEstimado <= 0 && (!result.Itens.Any() || result.Itens.Sum(x => x.Valor) <= 0))
            {
                 history.Status = "Failed";
                 history.ParseError = "Empty or zero value receipt detected";
                 await _historyRepository.AddAsync(history);
                 return null;
            }
            
            history.Status = "Success";
            await _historyRepository.AddAsync(history);

            return result;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            history.LatencyMs = stopwatch.ElapsedMilliseconds;
            history.Status = "Error";
            history.ParseError = ex.Message;
            await _historyRepository.AddAsync(history);
            
            _logger.LogError(ex, "{CorrelationId} Unexpected error in GroqVisionService", correlationId);
            return null;
        }
    }

    private bool CheckRateLimit(int usuarioId)
    {
        var now = DateTime.UtcNow;
        var minuteKey = $"scan_limit_min_{usuarioId}_{now:yyyyMMddHHmm}";
        var dailyKey = $"scan_limit_day_{usuarioId}_{now:yyyyMMdd}";

        _cache.TryGetValue(minuteKey, out int minuteCount);
        _cache.TryGetValue(dailyKey, out int dailyCount);

        if (minuteCount >= MaxMinuteScans || dailyCount >= MaxDailyScans)
            return false;

        _cache.Set(minuteKey, minuteCount + 1, TimeSpan.FromMinutes(1));
        _cache.Set(dailyKey, dailyCount + 1, TimeSpan.FromDays(1));

        return true;
    }

    private string SanitizeJson(string content)
    {
        if (string.IsNullOrWhiteSpace(content)) return "{}";
        
        var match = Regex.Match(content, @"```json\s*(\{.*\})\s*```", RegexOptions.Singleline);
        if (match.Success)
        {
            return match.Groups[1].Value;
        }

        var start = content.IndexOf('{');
        var end = content.LastIndexOf('}');
        
        if (start >= 0 && end > start)
        {
            return content.Substring(start, end - start + 1);
        }

        return content;
    }

    private string GetSystemPrompt()
    {
        var hoje = DateTime.Today.ToString("yyyy-MM-dd");
        return $@"Você é um assistente financeiro de elite. Sua tarefa é converter imagens em JSON estruturado com base no TIPO de imagem.

EXEMPLO 1 (CUPOM FISCAL / NOTA):
- Entrada: Imagem de um cupom de supermercado ou farmácia.
- Ação: Agrupar TUDO em um único item.
- Resultado Itens: [{{ ""Descricao"": ""Compras - [Nome]"", ""Valor"": [Total], ""CategoriaSugerida"": ""Mercado"", ""Tipo"": ""Saida"" }}]

EXEMPLO 2 (LISTA MANUSCRITA / CADERNO):
- Entrada: Foto de uma lista escrita à mão.
- Ação: Detalhar cada linha da lista.
- Resultado Itens: [{{ ""Descricao"": ""Item 1"", ... }}, {{ ""Descricao"": ""Item 2"", ... }}]

REGRAS RÍGIDAS:
1. SE FOR CUPOM FISCAL: A lista 'Itens' deve conter EXATAMENTE 1 ITEM com o valor total da nota. Use a categoria que melhor descreve a loja (Mercado, Farmácia, Restaurante, Combustível).
2. SE FOR LISTA MANUAL: Transcreva cada linha como um item separado.
3. DATA: Formato YYYY-MM-DD. Se ausente ou ilegível na imagem, use OBRIGATORIAMENTE a data de hoje: {hoje}.
4. VALORES (Decimal vs Inteiro): Se um número NÃO tiver separador (vírgula ou ponto), trate como VALOR INTEIRO. Exemplo: '1300' é 1300.00, não 13.00. Use decimais apenas se houver separador explícito na imagem.
5. JSON: Retorne APENAS o JSON, sem explicações.

OUTPUT SCHEMA:
{{
  ""nomeLista"": ""Titulo Curto"",
  ""data"": ""YYYY-MM-DD"",
  ""totalEstimado"": 0.00,
  ""itens"": [
    {{ ""descricao"": ""Nome Item"", ""valor"": 0.00, ""categoriaSugerida"": ""Categoria"", ""tipo"": ""Saida"" }}
  ]
}}";
    }
}
