namespace ControleFinanceiro.Application.DTOs;

public record LancamentoRequest(
    string Descricao, 
    decimal Valor, 
    DateTime Data, 
    string Tipo, 
    int CategoriaId);

public record LancamentoResponse(
    int Id, 
    string Descricao, 
    decimal Valor, 
    DateTime Data, 
    string Tipo, 
    int CategoriaId,
    string CategoriaNome);
