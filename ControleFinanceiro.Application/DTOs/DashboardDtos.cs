namespace ControleFinanceiro.Application.DTOs;

public record DashboardSummaryResponse(
    decimal TotalEntradas, 
    decimal TotalSaidas, 
    decimal Saldo);
