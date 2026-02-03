using System.Collections.Generic;
using System.Threading.Tasks;
using ControleFinanceiro.Domain.Entities;

namespace ControleFinanceiro.Domain.Interfaces;

public interface IRecorrenteRepository
{
    Task<IEnumerable<Recorrente>> GetAllAsync(int usuarioId);
    Task<Recorrente?> GetByIdAsync(int id, int usuarioId);
    Task<int> CreateAsync(Recorrente recorrente);
    Task UpdateAsync(Recorrente recorrente);
    Task DeleteAsync(int id, int usuarioId);
}
