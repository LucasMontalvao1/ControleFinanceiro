namespace ControleFinanceiro.Domain.DTOs;

public record ReceiptAnalysisResponse(
    string NomeLista,
    DateTime Data,
    List<ReceiptItemDto> Itens,
    decimal TotalEstimado
);

public record ReceiptItemDto(
    string Descricao,
    decimal Valor,
    string CategoriaSugerida,
    string Tipo
);
