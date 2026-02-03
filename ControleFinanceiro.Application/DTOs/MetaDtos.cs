namespace ControleFinanceiro.Application.DTOs;

public record MetaRequest(
    int CategoriaId,
    decimal ValorLimite,
    int Mes,
    int Ano);

public record MetaResponse(
    int Id,
    int CategoriaId,
    string CategoriaNome,
    decimal ValorLimite,
    decimal ValorRealizado,
    int Mes,
    int Ano);
