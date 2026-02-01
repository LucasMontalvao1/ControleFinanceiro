using System.Data;
using ControleFinanceiro.Domain.Interfaces;
using Microsoft.Extensions.Configuration;
using MySql.Data.MySqlClient;

namespace ControleFinanceiro.Infrastructure.Persistence;

public class MySqlDbConnectionFactory : IDbConnectionFactory
{
    private readonly IConfiguration _configuration;
    private readonly string _connectionString;

    public MySqlDbConnectionFactory(IConfiguration configuration)
    {
        _configuration = configuration;
        _connectionString = _configuration.GetConnectionString("DefaultConnection") 
                            ?? throw new ArgumentNullException("DefaultConnection string is missing.");
    }

    public IDbConnection CreateConnection()
    {
        return new MySqlConnection(_connectionString);
    }
}
