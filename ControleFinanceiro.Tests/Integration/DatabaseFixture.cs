using MySql.Data.MySqlClient;
using Testcontainers.MySql;
using Xunit;

namespace ControleFinanceiro.Tests.Integration;

public class DatabaseFixture : IAsyncLifetime
{
    private MySqlContainer? _container;
    public string ConnectionString { get; private set; } = string.Empty;

    public async Task InitializeAsync()
    {
        _container = new MySqlBuilder()
            .WithImage("mysql:8.0")
            .WithDatabase("controle_financeiro_test")
            .WithUsername("test_user")
            .WithPassword("test_password")
            .Build();

        await _container.StartAsync();
        ConnectionString = _container.GetConnectionString();

        await InitializeDatabaseSchema();
    }

    private async Task InitializeDatabaseSchema()
    {
        using var connection = new MySqlConnection(ConnectionString);
        await connection.OpenAsync();

        var createTablesScript = @"
            CREATE TABLE IF NOT EXISTS Usuarios (
                Id INT AUTO_INCREMENT PRIMARY KEY,
                Nome VARCHAR(100) NOT NULL,
                Username VARCHAR(50) NOT NULL UNIQUE,
                Email VARCHAR(150) NOT NULL UNIQUE,
                PasswordHash VARCHAR(255) NOT NULL,
                CreatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP
            );

            CREATE TABLE IF NOT EXISTS Categorias (
                Id INT AUTO_INCREMENT PRIMARY KEY,
                Nome VARCHAR(50) NOT NULL,
                Tipo VARCHAR(10) NOT NULL,
                UsuarioId INT NOT NULL,
                IsDefault BOOLEAN NOT NULL DEFAULT FALSE,
                CreatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
                CONSTRAINT FK_Categorias_Usuarios FOREIGN KEY (UsuarioId) REFERENCES Usuarios(Id) ON DELETE CASCADE
            );

            CREATE TABLE IF NOT EXISTS Lancamentos (
                Id INT AUTO_INCREMENT PRIMARY KEY,
                Descricao VARCHAR(200) NOT NULL,
                Valor DECIMAL(18,2) NOT NULL,
                Data DATETIME NOT NULL,
                Tipo VARCHAR(10) NOT NULL,
                UsuarioId INT NOT NULL,
                CategoriaId INT NOT NULL,
                CreatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
                CONSTRAINT FK_Lancamentos_Usuarios FOREIGN KEY (UsuarioId) REFERENCES Usuarios(Id) ON DELETE CASCADE,
                CONSTRAINT FK_Lancamentos_Categorias FOREIGN KEY (CategoriaId) REFERENCES Categorias(Id)
            );

            CREATE TABLE IF NOT EXISTS Recorrentes (
                Id INT AUTO_INCREMENT PRIMARY KEY,
                UsuarioId INT NOT NULL,
                CategoriaId INT NOT NULL,
                Descricao VARCHAR(255) NOT NULL,
                Valor DECIMAL(18, 2) NOT NULL,
                DiaVencimento INT NOT NULL,
                Tipo VARCHAR(10) NOT NULL,
                Ativo BOOLEAN DEFAULT TRUE,
                CreatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
                FOREIGN KEY (UsuarioId) REFERENCES Usuarios(Id),
                FOREIGN KEY (CategoriaId) REFERENCES Categorias(Id)
            );

            CREATE TABLE IF NOT EXISTS Metas (
                Id INT AUTO_INCREMENT PRIMARY KEY,
                UsuarioId INT NOT NULL,
                CategoriaId INT NOT NULL,
                ValorLimite DECIMAL(18, 2) NOT NULL,
                Mes INT NOT NULL,
                Ano INT NOT NULL,
                UNIQUE KEY (UsuarioId, CategoriaId, Mes, Ano),
                FOREIGN KEY (UsuarioId) REFERENCES Usuarios(Id),
                FOREIGN KEY (CategoriaId) REFERENCES Categorias(Id)
            );

            -- V006 Alterations
            ALTER TABLE Lancamentos ADD COLUMN RecorrenteId INT NULL;
            ALTER TABLE Lancamentos ADD CONSTRAINT FK_Lancamentos_Recorrentes FOREIGN KEY (RecorrenteId) REFERENCES Recorrentes(Id) ON DELETE SET NULL;
        ";

        using var command = new MySqlCommand(createTablesScript, connection);
        await command.ExecuteNonQueryAsync();
    }

    public async Task DisposeAsync()
    {
        if (_container != null)
        {
            await _container.DisposeAsync();
        }
    }
}

[CollectionDefinition("Database")]
public class DatabaseCollection : ICollectionFixture<DatabaseFixture>
{
}
