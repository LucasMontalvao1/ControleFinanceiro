using System.Reflection;

namespace ControleFinanceiro.Infrastructure.Persistence;

public interface ISqlProvider
{
    string GetSql(string entity, string queryName);
}

public class EmbeddedSqlProvider : ISqlProvider
{
    private readonly Assembly _assembly;
    private const string Namespace = "ControleFinanceiro.Infrastructure.Persistence.Queries";

    public EmbeddedSqlProvider()
    {
        _assembly = typeof(EmbeddedSqlProvider).Assembly;
    }

    public string GetSql(string entity, string queryName)
    {
        var resourceName = $"{Namespace}.{entity}.{queryName}.sql";
        
        using var stream = _assembly.GetManifestResourceStream(resourceName);
        if (stream == null)
        {
            throw new FileNotFoundException($"Recurso SQL não encontrado: {resourceName}");
        }

        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }
}
