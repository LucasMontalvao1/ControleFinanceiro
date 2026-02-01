namespace ControleFinanceiro.Application.DTOs;

public record RegisterRequest(string Nome, string Username, string Email, string Password);
public record LoginRequest(string Username, string Password);
public record AuthResponse(string Token, string Nome, string Email);
