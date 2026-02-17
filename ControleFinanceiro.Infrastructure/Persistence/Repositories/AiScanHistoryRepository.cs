using System.Threading.Tasks;
using Dapper;
using ControleFinanceiro.Domain.Entities;
using ControleFinanceiro.Domain.Interfaces;

namespace ControleFinanceiro.Infrastructure.Persistence.Repositories;

public class AiScanHistoryRepository : IAiScanHistoryRepository
{
    private readonly DbSession _dbSession;

    public AiScanHistoryRepository(DbSession dbSession)
    {
        _dbSession = dbSession;
    }

    public async Task<int> AddAsync(AiScanHistory history)
    {
        var query = @"
            INSERT INTO AiScanHistory (CorrelationId, UsuarioId, Status, LatencyMs, RawJson, ParseError, ProcessadoEm)
            VALUES (@CorrelationId, @UsuarioId, @Status, @LatencyMs, @RawJson, @ParseError, @ProcessadoEm);
            SELECT LAST_INSERT_ID();";

        return await _dbSession.Connection.ExecuteScalarAsync<int>(query, history, _dbSession.Transaction);
    }
}
