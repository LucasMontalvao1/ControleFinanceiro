using Dapper;
using ControleFinanceiro.Domain.Interfaces;
using Serilog;
using System.Reflection;
using System.Data;
using System.Linq;

namespace ControleFinanceiro.Infrastructure.Persistence;

public class DatabaseInitializer
{
    private readonly IDbConnectionFactory _connectionFactory;

    public DatabaseInitializer(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public void Initialize()
    {
        Log.Information("Iniciando a inicialização automática do banco de dados...");

        EnsureDatabaseExists();

        using var connection = _connectionFactory.CreateConnection();
        connection.Open();

        // Criar tabela de histórico de migrações se necessário
        var createHistoryTableSql = @"
            CREATE TABLE IF NOT EXISTS HistoricoMigracoes (
                Id INT AUTO_INCREMENT PRIMARY KEY,
                NomeArquivo VARCHAR(255) NOT NULL UNIQUE,
                DataAplicacao DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP
            );";
        connection.Execute(createHistoryTableSql);

        var scriptsPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Persistence", "Scripts");
        
        if (!Directory.Exists(scriptsPath) || !Directory.GetFiles(scriptsPath, "*.sql").Any())
        {
            scriptsPath = Path.Combine(Directory.GetCurrentDirectory(), "..", "ControleFinanceiro.Infrastructure", "Persistence", "Scripts");
        }

        Log.Information("Buscando scripts SQL em: {Path}", scriptsPath);

        if (!Directory.Exists(scriptsPath))
        {
            Log.Warning("Pasta de scripts SQL não encontrada em: {Path}", scriptsPath);
            return;
        }

        var scripts = Directory.GetFiles(scriptsPath, "*.sql")
                               .OrderBy(f => f)
                               .ToList();

        foreach (var scriptPath in scripts)
        {
            var fileName = Path.GetFileName(scriptPath);
            
            var alreadyApplied = connection.ExecuteScalar<int>(
                "SELECT COUNT(1) FROM HistoricoMigracoes WHERE NomeArquivo = @fileName", 
                new { fileName }) > 0;

            if (!alreadyApplied)
            {
                Log.Information("Aplicando migração: {FileName}", fileName);
                var sql = File.ReadAllText(scriptPath);
                
                using var transaction = connection.BeginTransaction();
                try
                {
                    connection.Execute(sql, transaction: transaction);
                    connection.Execute(
                        "INSERT INTO HistoricoMigracoes (NomeArquivo) VALUES (@fileName)", 
                        new { fileName }, 
                        transaction: transaction);
                    
                    transaction.Commit();
                    Log.Information("Migração {FileName} aplicada com sucesso.", fileName);
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    Log.Error(ex, "Falha ao aplicar a migração {FileName}.", fileName);
                    throw;
                }
            }
        }

        Log.Information("Inicialização do banco de dados finalizada.");
    }

    private void EnsureDatabaseExists()
    {
        using var connection = _connectionFactory.CreateConnection();
        var builder = new MySql.Data.MySqlClient.MySqlConnectionStringBuilder(connection.ConnectionString);
        var databaseName = builder.Database;
        
        // Conectar ao MySQL sem especificar database
        builder.Database = "";
        using var masterConnection = new MySql.Data.MySqlClient.MySqlConnection(builder.ConnectionString);
        masterConnection.Open();
        
        masterConnection.Execute($"CREATE DATABASE IF NOT EXISTS `{databaseName}`;");
        Log.Information("Garantido que o banco de dados '{Database}' existe.", databaseName);
    }
}
