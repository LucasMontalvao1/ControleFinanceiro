using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ControleFinanceiro.Application.DTOs;
using ControleFinanceiro.Domain.Entities;
using ControleFinanceiro.Domain.Interfaces;

namespace ControleFinanceiro.Application.Services;

public interface IMetaService
{
    Task<Result<IEnumerable<MetaResponse>>> GetAllAsync(int usuarioId, int mes, int ano);
    Task<Result<int>> CreateOrUpdateAsync(int usuarioId, MetaRequest request);
    Task<Result<bool>> DeleteAsync(int id, int usuarioId);
}

public class MetaService : IMetaService
{
    private readonly IMetaRepository _metaRepository;
    private readonly ILancamentoRepository _lancamentoRepository;
    private readonly ICategoriaRepository _categoriaRepository;

    public MetaService(IMetaRepository metaRepository, ILancamentoRepository lancamentoRepository, ICategoriaRepository categoriaRepository)
    {
        _metaRepository = metaRepository;
        _lancamentoRepository = lancamentoRepository;
        _categoriaRepository = categoriaRepository;
    }

    public async Task<Result<IEnumerable<MetaResponse>>> GetAllAsync(int usuarioId, int mes, int ano)
    {
        var metas = await _metaRepository.GetAllAsync(usuarioId, mes, ano);
        var categorias = await _categoriaRepository.GetAllByUsuarioIdAsync(usuarioId);
        var categoriasDespesa = categorias.Where(c => c.Tipo == "Despesa");

        var startDate = new DateTime(ano, mes, 1);
        var endDate = new DateTime(ano, mes, DateTime.DaysInMonth(ano, mes), 23, 59, 59);
        var lancamentos = await _lancamentoRepository.GetAllByUsuarioIdAsync(usuarioId, startDate, endDate);
        
        var response = categoriasDespesa.Select(c => {
            var m = metas.FirstOrDefault(x => x.CategoriaId == c.Id);
            
            return new MetaResponse(
                m?.Id ?? 0,
                c.Id,
                c.Nome,
                m?.ValorLimite ?? 0,
                lancamentos.Where(l => l.CategoriaId == c.Id && l.Tipo == "Saida").Sum(l => l.Valor),
                mes,
                ano
            );
        });

        return Result<IEnumerable<MetaResponse>>.Ok(response);
    }

    public async Task<Result<int>> CreateOrUpdateAsync(int usuarioId, MetaRequest request)
    {
        var existing = await _metaRepository.GetAllAsync(usuarioId, request.Mes, request.Ano);
        var meta = existing.FirstOrDefault(m => m.CategoriaId == request.CategoriaId);

        if (meta != null)
        {
            meta.ValorLimite = request.ValorLimite;
            await _metaRepository.UpdateAsync(meta);
            return Result<int>.Ok(meta.Id);
        }
        else
        {
            var newMeta = new Meta
            {
                UsuarioId = usuarioId,
                CategoriaId = request.CategoriaId,
                ValorLimite = request.ValorLimite,
                Mes = request.Mes,
                Ano = request.Ano
            };
            var id = await _metaRepository.CreateAsync(newMeta);
            return Result<int>.Ok(id);
        }
    }

    public async Task<Result<bool>> DeleteAsync(int id, int usuarioId)
    {
        await _metaRepository.DeleteAsync(id, usuarioId);
        return Result<bool>.Ok(true);
    }
}
