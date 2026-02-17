using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using ControleFinanceiro.Application.Services;
using ControleFinanceiro.Domain.Entities;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.IdentityModel.Tokens;
using Moq;

namespace ControleFinanceiro.Tests.Integration.API;

public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    public Mock<IAuthService> AuthServiceMock { get; } = new();
    public Mock<ICategoriaService> CategoriaServiceMock { get; } = new();
    public Mock<ILancamentoService> LancamentoServiceMock { get; } = new();
    public Mock<IMetaService> MetaServiceMock { get; } = new();
    public Mock<IRecorrenteService> RecorrenteServiceMock { get; } = new();
    public Mock<ControleFinanceiro.Domain.Interfaces.IAiReceiptService> AiReceiptServiceMock { get; } = new();
    public Mock<ControleFinanceiro.Domain.Interfaces.IAiScanHistoryRepository> AiScanHistoryRepositoryMock { get; } = new();

    public string JwtSecret { get; } = "SuperSecretKeyForTestingPurposesOnly123!";
    public string JwtIssuer { get; } = "ControleFinanceiroTest";
    public string JwtAudience { get; } = "ControleFinanceiroTest";

    public CustomWebApplicationFactory()
    {
        // Force JWT settings via Environment Variables to ensure they override appsettings.json
        Environment.SetEnvironmentVariable("JwtSettings__Secret", JwtSecret);
        Environment.SetEnvironmentVariable("JwtSettings__Issuer", JwtIssuer);
        Environment.SetEnvironmentVariable("JwtSettings__Audience", JwtAudience);
        Environment.SetEnvironmentVariable("JwtSettings__ExpirationMinutes", "60");
        Environment.SetEnvironmentVariable("EnableGlobalErrorHandling", "false");
        
        // Mock Groq API Key to avoid DI failure
        Environment.SetEnvironmentVariable("Groq__ApiKey", "test-api-key");
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureAppConfiguration((context, config) =>
        {
            // Also add to configuration builder as backup
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                { "JwtSettings:Secret", JwtSecret },
                { "JwtSettings:Issuer", JwtIssuer },
                { "JwtSettings:Audience", JwtAudience },
                { "JwtSettings:ExpirationMinutes", "60" },
                { "EnableGlobalErrorHandling", "false" },
                { "Groq:ApiKey", "test-api-key" }
            });
        });

        builder.UseTestServer(options => options.AllowSynchronousIO = true);

        builder.ConfigureTestServices(services =>
        {
            // Remove existing registrations
            services.RemoveAll<IAuthService>();
            services.RemoveAll<ICategoriaService>();
            services.RemoveAll<ILancamentoService>();
            services.RemoveAll<IMetaService>();
            services.RemoveAll<IRecorrenteService>();
            services.RemoveAll<ControleFinanceiro.Domain.Interfaces.IAiReceiptService>();
            services.RemoveAll<ControleFinanceiro.Domain.Interfaces.IAiScanHistoryRepository>();

            // Register mocks
            services.AddSingleton(AuthServiceMock.Object);
            services.AddSingleton(CategoriaServiceMock.Object);
            services.AddSingleton(LancamentoServiceMock.Object);
            services.AddSingleton(MetaServiceMock.Object);
            services.AddSingleton(RecorrenteServiceMock.Object);
            services.AddSingleton(AiReceiptServiceMock.Object);
            services.AddSingleton(AiScanHistoryRepositoryMock.Object);
        });
    }

    public string GenerateJwtToken(int userId, string email, string role = "User")
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.UTF8.GetBytes(JwtSecret);
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
                new Claim(ClaimTypes.Email, email),
                new Claim(ClaimTypes.Role, role),
                new Claim("unique_name", email),
                new Claim(JwtRegisteredClaimNames.Sub, userId.ToString()) // Explicitly add 'sub' just in case
            }),
            Expires = DateTime.UtcNow.AddMinutes(60),
            Issuer = JwtIssuer,
            Audience = JwtAudience,
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };
        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }
}

public static class ServiceCollectionExtensions
{
    public static void RemoveAll<T>(this IServiceCollection services)
    {
        var descriptors = services.Where(descriptor => descriptor.ServiceType == typeof(T)).ToList();
        foreach (var descriptor in descriptors)
        {
            services.Remove(descriptor);
        }
    }
}
