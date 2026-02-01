using ControleFinanceiro.Domain.Entities;

namespace ControleFinanceiro.Domain.Interfaces;

public interface IUsuarioRepository
{
    Task<Usuario?> GetByIdAsync(int id);
    Task<Usuario?> GetByEmailAsync(string email);
    Task<Usuario?> GetByUsernameAsync(string username);
    Task AddAsync(Usuario usuario);
}
