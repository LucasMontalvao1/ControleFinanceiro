using System.Threading.Tasks;
using ControleFinanceiro.Domain.Entities;

namespace ControleFinanceiro.Domain.Interfaces;

public interface IAiScanHistoryRepository
{
    Task<int> AddAsync(AiScanHistory history);
}
