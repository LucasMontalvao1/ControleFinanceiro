using System.Collections.Generic;
using System.Threading.Tasks;
using ControleFinanceiro.Domain.Entities;

namespace ControleFinanceiro.Domain.Interfaces;

public interface IMetaRepository
{
    Task<IEnumerable<Meta>> GetAllAsync(int usuarioId, int mes, int ano);
    Task<Meta?> GetByIdAsync(int id, int usuarioId);
    Task<int> CreateAsync(Meta meta);
    Task UpdateAsync(Meta meta);
    Task DeleteAsync(int id, int usuarioId);
}
