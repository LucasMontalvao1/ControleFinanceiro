using Serilog;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using FluentValidation;
using FluentValidation.AspNetCore;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.File("logs/log-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

try
{
    Log.Information("Iniciando a API de Controle Financeiro...");
    var builder = WebApplication.CreateBuilder(args);

    // Configurar Serilog
    builder.Host.UseSerilog();

    // CORS
    builder.Services.AddCors(options =>
    {
        options.AddPolicy("DefaultPolicy", policy =>
        {
            policy.AllowAnyOrigin()
                  .AllowAnyMethod()
                  .AllowAnyHeader();
        });
    });

    // Add services to the container.
    builder.Services.AddSingleton<ControleFinanceiro.Domain.Interfaces.IDbConnectionFactory, ControleFinanceiro.Infrastructure.Persistence.MySqlDbConnectionFactory>();
    builder.Services.AddScoped<ControleFinanceiro.Infrastructure.Persistence.DbSession>();
    builder.Services.AddSingleton<ControleFinanceiro.Infrastructure.Persistence.ISqlProvider, ControleFinanceiro.Infrastructure.Persistence.EmbeddedSqlProvider>();
    builder.Services.AddSingleton<ControleFinanceiro.Infrastructure.Persistence.DatabaseInitializer>();
    builder.Services.AddScoped<ControleFinanceiro.Domain.Interfaces.IUsuarioRepository, ControleFinanceiro.Infrastructure.Persistence.Repositories.UsuarioRepository>();
    builder.Services.AddScoped<ControleFinanceiro.Domain.Interfaces.ICategoriaRepository, ControleFinanceiro.Infrastructure.Persistence.Repositories.CategoriaRepository>();
    builder.Services.AddScoped<ControleFinanceiro.Domain.Interfaces.ILancamentoRepository, ControleFinanceiro.Infrastructure.Persistence.Repositories.LancamentoRepository>();
    builder.Services.AddScoped<ControleFinanceiro.Domain.Interfaces.IRecorrenteRepository, ControleFinanceiro.Infrastructure.Persistence.Repositories.RecorrenteRepository>();
    builder.Services.AddScoped<ControleFinanceiro.Domain.Interfaces.IMetaRepository, ControleFinanceiro.Infrastructure.Persistence.Repositories.MetaRepository>();
    builder.Services.AddScoped<ControleFinanceiro.Application.Services.IAuthService, ControleFinanceiro.Application.Services.AuthService>();
    builder.Services.AddScoped<ControleFinanceiro.Application.Services.ICategoriaService, ControleFinanceiro.Application.Services.CategoriaService>();
    builder.Services.AddScoped<ControleFinanceiro.Application.Services.ILancamentoService, ControleFinanceiro.Application.Services.LancamentoService>();
    builder.Services.AddScoped<ControleFinanceiro.Application.Services.IRecorrenteService, ControleFinanceiro.Application.Services.RecorrenteService>();
    builder.Services.AddScoped<ControleFinanceiro.Application.Services.IMetaService, ControleFinanceiro.Application.Services.MetaService>();

    // FluentValidation
    builder.Services.AddFluentValidationAutoValidation();
    builder.Services.AddValidatorsFromAssemblyContaining<ControleFinanceiro.Application.Validators.RegisterRequestValidator>();

    // Configuração JWT
    var jwtSettings = builder.Configuration.GetSection("JwtSettings");
    var key = Encoding.UTF8.GetBytes(jwtSettings.GetValue<string>("Secret")!);

    System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();

    builder.Services.AddAuthentication(x =>
    {
        x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.RequireHttpsMetadata = false;
        options.SaveToken = true;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSettings.GetValue<string>("Issuer"),
            ValidAudience = jwtSettings.GetValue<string>("Audience"),
            IssuerSigningKey = new SymmetricSecurityKey(key),
            NameClaimType = "unique_name", 
            RoleClaimType = "role" 
        };
    });

    builder.Services.AddControllers();
    // Swagger/OpenAPI setup for .NET 8
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen();

    var app = builder.Build();

    // Inicializar Banco de Dados
    using (var scope = app.Services.CreateScope())
    {
        var initializer = scope.ServiceProvider.GetRequiredService<ControleFinanceiro.Infrastructure.Persistence.DatabaseInitializer>();
        initializer.Initialize();
    }

    // Middleware de Erro Global
    app.UseMiddleware<ControleFinanceiro.API.Middleware.GlobalErrorHandlingMiddleware>();

    // Configure the HTTP request pipeline.
    // Enable Swagger in ALL environments for Render testing
    app.UseSwagger();
    app.UseSwaggerUI();

    app.UseCors("DefaultPolicy");
    app.UseHttpsRedirection();

    app.UseAuthentication();
    app.UseAuthorization();

    app.MapControllers();

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "A API falhou ao iniciar!");
}
finally
{
    Log.CloseAndFlush();
}
