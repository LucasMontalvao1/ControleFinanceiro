using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ControleFinanceiro.Application.DTOs;
using ControleFinanceiro.Domain.Entities;
using ControleFinanceiro.Domain.Interfaces;

namespace ControleFinanceiro.Application.Services;

public interface IRecorrenteService
{
    Task<Result<IEnumerable<RecorrenteResponse>>> GetAllAsync(int usuarioId);
    Task<Result<RecorrenteResponse>> GetByIdAsync(int id, int usuarioId);
    Task<Result<int>> CreateAsync(int usuarioId, RecorrenteRequest request);
    Task<Result<bool>> UpdateAsync(int id, int usuarioId, RecorrenteRequest request);
    Task<Result<bool>> DeleteAsync(int id, int usuarioId);
    Task<Result<int>> ApplyToMonthAsync(int usuarioId, int mes, int ano);
}

public class RecorrenteService : IRecorrenteService
{
    private readonly IRecorrenteRepository _recorrenteRepository;
    private readonly ILancamentoRepository _lancamentoRepository;

    public RecorrenteService(IRecorrenteRepository recorrenteRepository, ILancamentoRepository lancamentoRepository)
    {
        _recorrenteRepository = recorrenteRepository;
        _lancamentoRepository = lancamentoRepository;
    }

    public async Task<Result<IEnumerable<RecorrenteResponse>>> GetAllAsync(int usuarioId)
    {
        var recorrentes = await _recorrenteRepository.GetAllAsync(usuarioId);
        var response = recorrentes.Select(r => MapToResponse(r));
        return Result<IEnumerable<RecorrenteResponse>>.Ok(response);
    }

    public async Task<Result<RecorrenteResponse>> GetByIdAsync(int id, int usuarioId)
    {
        var recorrente = await _recorrenteRepository.GetByIdAsync(id, usuarioId);
        if (recorrente == null) return Result<RecorrenteResponse>.Fail("Recorrência não encontrada.");
        return Result<RecorrenteResponse>.Ok(MapToResponse(recorrente));
    }

    public async Task<Result<int>> CreateAsync(int usuarioId, RecorrenteRequest request)
    {
        var recorrente = new Recorrente
        {
            UsuarioId = usuarioId,
            CategoriaId = request.CategoriaId,
            Descricao = request.Descricao,
            Valor = request.Valor,
            DiaVencimento = request.DiaVencimento,
            Tipo = request.Tipo,
            Ativo = request.Ativo
        };

        var id = await _recorrenteRepository.CreateAsync(recorrente);
        return Result<int>.Ok(id);
    }

    public async Task<Result<bool>> UpdateAsync(int id, int usuarioId, RecorrenteRequest request)
    {
        var recorrente = await _recorrenteRepository.GetByIdAsync(id, usuarioId);
        if (recorrente == null) return Result<bool>.Fail("Recorrência não encontrada.");

        recorrente.CategoriaId = request.CategoriaId;
        recorrente.Descricao = request.Descricao;
        recorrente.Valor = request.Valor;
        recorrente.DiaVencimento = request.DiaVencimento;
        recorrente.Tipo = request.Tipo;
        recorrente.Ativo = request.Ativo;

        await _recorrenteRepository.UpdateAsync(recorrente);
        return Result<bool>.Ok(true);
    }

    public async Task<Result<bool>> DeleteAsync(int id, int usuarioId)
    {
        await _recorrenteRepository.DeleteAsync(id, usuarioId);
        return Result<bool>.Ok(true);
    }

    public async Task<Result<int>> ApplyToMonthAsync(int usuarioId, int mes, int ano)
    {
        var recorrentes = await _recorrenteRepository.GetAllAsync(usuarioId);
        var ativos = recorrentes.Where(r => r.Ativo).ToList();
        
        // Busca lançamentos do mês que já possuem vínculo com recorrência
        var startDate = new DateTime(ano, mes, 1);
        var endDate = new DateTime(ano, mes, DateTime.DaysInMonth(ano, mes), 23, 59, 59);
        var lancamentosExistentes = await _lancamentoRepository.GetAllByUsuarioIdAsync(usuarioId, startDate, endDate);
        
        var idsJaAplicados = lancamentosExistentes
            .Where(l => l.RecorrenteId.HasValue)
            .Select(l => l.RecorrenteId.Value)
            .Distinct()
            .ToHashSet();

        int count = 0;
        foreach (var r in ativos)
        {
            if (idsJaAplicados.Contains(r.Id)) continue;

            var dataPrevista = new DateTime(ano, mes, Math.Min(r.DiaVencimento, DateTime.DaysInMonth(ano, mes)), 12, 0, 0, DateTimeKind.Utc);
            
            await _lancamentoRepository.AddAsync(new Lancamento
            {
                UsuarioId = usuarioId,
                CategoriaId = r.CategoriaId,
                Descricao = r.Descricao,
                Valor = r.Valor,
                Data = dataPrevista,
                Tipo = r.Tipo,
                RecorrenteId = r.Id,
                CreatedAt = DateTime.UtcNow
            });
            count++;
        }

        return Result<int>.Ok(count);
    }

    private RecorrenteResponse MapToResponse(Recorrente r) => new RecorrenteResponse(
        r.Id, r.CategoriaId, r.CategoriaNome, r.Descricao, r.Valor, r.DiaVencimento, r.Tipo, r.Ativo);
}
