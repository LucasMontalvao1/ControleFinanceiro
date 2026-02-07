using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using ControleFinanceiro.Application.Configuration;
using ControleFinanceiro.Application.DTOs;
using ControleFinanceiro.Domain.Entities;
using ControleFinanceiro.Domain.Interfaces;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace ControleFinanceiro.Application.Services;

public interface IAuthService
{
    Task<Result<AuthResponse>> RegisterAsync(RegisterRequest request);
    Task<Result<AuthResponse>> LoginAsync(LoginRequest request);
}

public class AuthService : IAuthService
{
    private readonly IUsuarioRepository _usuarioRepository;
    private readonly JwtSettings _jwtSettings;

    public AuthService(IUsuarioRepository usuarioRepository, IOptions<JwtSettings> jwtOptions)
    {
        _usuarioRepository = usuarioRepository;
        _jwtSettings = jwtOptions.Value;
    }

    public async Task<Result<AuthResponse>> RegisterAsync(RegisterRequest request)
    {
        var existingEmail = await _usuarioRepository.GetByEmailAsync(request.Email);
        if (existingEmail != null)
            return Result<AuthResponse>.Fail("E-mail já está em uso.");

        var existingUsername = await _usuarioRepository.GetByUsernameAsync(request.Username);
        if (existingUsername != null)
            return Result<AuthResponse>.Fail("Nome de usuário já está em uso.");

        var user = new Usuario
        {
            Nome = request.Nome,
            Username = request.Username,
            Email = request.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password)
        };

        await _usuarioRepository.AddAsync(user);

        var token = GenerateJwtToken(user);
        return Result<AuthResponse>.Ok(new AuthResponse(token, user.Nome, user.Email));
    }

    public async Task<Result<AuthResponse>> LoginAsync(LoginRequest request)
    {
        var user = await _usuarioRepository.GetByUsernameAsync(request.Username);
        if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            return Result<AuthResponse>.Fail("Login ou senha inválidos.");

        var token = GenerateJwtToken(user);
        return Result<AuthResponse>.Ok(new AuthResponse(token, user.Nome, user.Email));
    }

    private string GenerateJwtToken(Usuario user)
    {
        var secretKey = _jwtSettings.Secret;
        if (string.IsNullOrEmpty(secretKey))
            throw new InvalidOperationException("Configuração do JWT ausente.");
        
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.UniqueName, user.Username),
            new Claim(JwtRegisteredClaimNames.Name, user.Nome),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var token = new JwtSecurityToken(
            issuer: _jwtSettings.Issuer,
            audience: _jwtSettings.Audience,
            claims: claims,
            expires: DateTime.Now.AddDays(_jwtSettings.ExpiryDays),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
