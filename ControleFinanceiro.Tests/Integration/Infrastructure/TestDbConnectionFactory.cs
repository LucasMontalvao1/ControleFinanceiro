using ControleFinanceiro.Domain.Interfaces;
using MySql.Data.MySqlClient;
using System.Data;

namespace ControleFinanceiro.Tests.Integration.Infrastructure;

public class TestDbConnectionFactory : IDbConnectionFactory
{
    private readonly string _connectionString;

    public TestDbConnectionFactory(string connectionString)
    {
        _connectionString = connectionString;
    }

    public IDbConnection CreateConnection()
    {
        return new MySqlConnection(_connectionString);
    }
}
