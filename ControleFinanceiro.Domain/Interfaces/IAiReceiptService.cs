using System.Threading.Tasks;
using ControleFinanceiro.Domain.DTOs;

namespace ControleFinanceiro.Domain.Interfaces;

public interface IAiReceiptService
{
    Task<ReceiptAnalysisResponse?> AnalyzeReceiptAsync(string base64Image, int usuarioId, string correlationId);
}
