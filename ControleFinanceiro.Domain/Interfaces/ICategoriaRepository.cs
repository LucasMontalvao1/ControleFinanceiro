using ControleFinanceiro.Domain.Entities;

namespace ControleFinanceiro.Domain.Interfaces;

public interface ICategoriaRepository
{
    Task<IEnumerable<Categoria>> GetAllByUsuarioIdAsync(int usuarioId);
    Task<Categoria?> GetByIdAsync(int id, int usuarioId);
    Task<int> AddAsync(Categoria categoria);
    Task UpdateAsync(Categoria categoria);
    Task DeleteAsync(int id, int usuarioId);
}
