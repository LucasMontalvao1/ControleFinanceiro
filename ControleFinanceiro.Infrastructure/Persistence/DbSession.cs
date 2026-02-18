using System.Data;
using ControleFinanceiro.Domain.Interfaces;

namespace ControleFinanceiro.Infrastructure.Persistence;

public sealed class DbSession : IDisposable
{
    public IDbConnection Connection { get; }
    public IDbTransaction? Transaction { get; private set; }

    public DbSession(IDbConnectionFactory connectionFactory)
    {
        Connection = connectionFactory.CreateConnection();
        if (Connection.State != ConnectionState.Open)
            Connection.Open();
    }

    public void BeginTransaction()
    {
        Transaction = Connection.BeginTransaction();
    }

    public void Commit()
    {
        Transaction?.Commit();
        DisposeTransaction();
    }

    public void Rollback()
    {
        Transaction?.Rollback();
        DisposeTransaction();
    }

    private void DisposeTransaction()
    {
        Transaction?.Dispose();
        Transaction = null;
    }

    public void Dispose()
    {
        Transaction?.Dispose();
        Connection?.Dispose();
    }
}
