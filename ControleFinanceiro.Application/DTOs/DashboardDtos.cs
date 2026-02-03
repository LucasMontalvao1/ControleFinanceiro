namespace ControleFinanceiro.Application.DTOs;

public record DashboardSummaryResponse(
    decimal TotalEntradas, 
    decimal TotalSaidas, 
    decimal Saldo,
    IEnumerable<CategoriaGraficoResponse> GastosPorCategoria,
    IEnumerable<EvolucaoDiariaResponse> EvolucaoMensal,
    IEnumerable<EvolucaoMensalGraficoResponse> EvolucaoAnual,
    IEnumerable<LancamentoResponse> LancamentosRecentes);

public record CategoriaGraficoResponse(string Categoria, decimal Valor);
public record EvolucaoDiariaResponse(DateTime Data, decimal Entradas, decimal Saidas);
public record EvolucaoMensalGraficoResponse(string Mes, decimal Entradas, decimal Saidas, decimal Saldo);
