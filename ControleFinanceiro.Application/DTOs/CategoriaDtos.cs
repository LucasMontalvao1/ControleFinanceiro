namespace ControleFinanceiro.Application.DTOs;

public record CategoriaRequest(string Nome, string Tipo);
public record CategoriaResponse(int Id, string Nome, string Tipo, bool IsDefault);
