using System.Data;

namespace ControleFinanceiro.Domain.Interfaces;

public interface IDbConnectionFactory
{
    IDbConnection CreateConnection();
}
