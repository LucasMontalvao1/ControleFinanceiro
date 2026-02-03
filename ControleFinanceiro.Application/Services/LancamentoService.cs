using ControleFinanceiro.Application.DTOs;
using ControleFinanceiro.Domain.Entities;
using ControleFinanceiro.Domain.Interfaces;

namespace ControleFinanceiro.Application.Services;

public interface ILancamentoService
{
    Task<Result<IEnumerable<LancamentoResponse>>> GetAllAsync(int usuarioId, DateTime? start = null, DateTime? end = null);
    Task<Result<LancamentoResponse>> GetByIdAsync(int id, int usuarioId);
    Task<Result<LancamentoResponse>> CreateAsync(LancamentoRequest request, int usuarioId);
    Task<Result> UpdateAsync(int id, LancamentoRequest request, int usuarioId);
    Task<Result> DeleteAsync(int id, int usuarioId);
    Task<Result<DashboardSummaryResponse>> GetDashboardSummaryAsync(int usuarioId, DateTime? start = null, DateTime? end = null);
}

public class LancamentoService : ILancamentoService
{
    private readonly ILancamentoRepository _lancamentoRepository;
    private readonly ICategoriaRepository _categoriaRepository;

    public LancamentoService(ILancamentoRepository lancamentoRepository, ICategoriaRepository categoriaRepository)
    {
        _lancamentoRepository = lancamentoRepository;
        _categoriaRepository = categoriaRepository;
    }

    public async Task<Result<IEnumerable<LancamentoResponse>>> GetAllAsync(int usuarioId, DateTime? start = null, DateTime? end = null)
    {
        var startDate = start ?? new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1);
        var endDate = end ?? startDate.AddMonths(1).AddDays(-1);

        var lancamentos = await _lancamentoRepository.GetAllByUsuarioIdAsync(usuarioId, startDate, endDate);
        var categorias = await _categoriaRepository.GetAllByUsuarioIdAsync(usuarioId);

        var response = lancamentos.Select(l => {
            var cat = categorias.FirstOrDefault(c => c.Id == l.CategoriaId);
            return new LancamentoResponse(
                l.Id, 
                l.Descricao, 
                l.Valor, 
                l.Data, 
                l.Tipo, 
                l.CategoriaId, 
                cat?.Nome ?? "Sem Categoria"
            );
        });

        return Result<IEnumerable<LancamentoResponse>>.Ok(response);
    }

    public async Task<Result<LancamentoResponse>> GetByIdAsync(int id, int usuarioId)
    {
        var l = await _lancamentoRepository.GetByIdAsync(id, usuarioId);
        if (l == null) return Result<LancamentoResponse>.Fail("Lançamento não encontrado.");

        var cat = await _categoriaRepository.GetByIdAsync(l.CategoriaId, usuarioId);
        
        return Result<LancamentoResponse>.Ok(new LancamentoResponse(
            l.Id, 
            l.Descricao, 
            l.Valor, 
            l.Data, 
            l.Tipo, 
            l.CategoriaId, 
            cat?.Nome ?? "Sem Categoria"
        ));
    }

    public async Task<Result<LancamentoResponse>> CreateAsync(LancamentoRequest request, int usuarioId)
    {
        var cat = await _categoriaRepository.GetByIdAsync(request.CategoriaId, usuarioId);
        if (cat == null) return Result<LancamentoResponse>.Fail("Categoria inválida.");

        var lancamento = new Lancamento
        {
            Descricao = request.Descricao,
            Valor = request.Valor,
            Data = request.Data,
            Tipo = request.Tipo,
            UsuarioId = usuarioId,
            CategoriaId = request.CategoriaId
        };

        var id = await _lancamentoRepository.AddAsync(lancamento);
        
        return Result<LancamentoResponse>.Ok(new LancamentoResponse(
            id, 
            lancamento.Descricao, 
            lancamento.Valor, 
            lancamento.Data, 
            lancamento.Tipo, 
            lancamento.CategoriaId, 
            cat.Nome
        ));
    }

    public async Task<Result> UpdateAsync(int id, LancamentoRequest request, int usuarioId)
    {
        var existing = await _lancamentoRepository.GetByIdAsync(id, usuarioId);
        if (existing == null) return Result.Fail("Lançamento não encontrado.");

        var cat = await _categoriaRepository.GetByIdAsync(request.CategoriaId, usuarioId);
        if (cat == null) return Result.Fail("Categoria inválida.");

        existing.Descricao = request.Descricao;
        existing.Valor = request.Valor;
        existing.Data = request.Data;
        existing.Tipo = request.Tipo;
        existing.CategoriaId = request.CategoriaId;

        await _lancamentoRepository.UpdateAsync(existing);
        return Result.Ok();
    }

    public async Task<Result> DeleteAsync(int id, int usuarioId)
    {
        var existing = await _lancamentoRepository.GetByIdAsync(id, usuarioId);
        if (existing == null) return Result.Fail("Lançamento não encontrado.");

        await _lancamentoRepository.DeleteAsync(id, usuarioId);
        return Result.Ok();
    }

    public async Task<Result<DashboardSummaryResponse>> GetDashboardSummaryAsync(int usuarioId, DateTime? start = null, DateTime? end = null)
    {
        var startDate = start ?? new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1);
        var endDate = end ?? startDate.AddMonths(1).AddDays(-1);

        var (totalEntradas, totalSaidas) = await _lancamentoRepository.GetSummaryAsync(usuarioId, startDate, endDate);
        
        var catResults = await _lancamentoRepository.GetExpensesByCategoryAsync(usuarioId, startDate, endDate);
        var trendResults = await _lancamentoRepository.GetMonthlyEvolutionAsync(usuarioId, startDate, endDate);
        var yearlyResults = await _lancamentoRepository.GetYearlyEvolutionAsync(usuarioId, endDate);
        var recentTransactions = await _lancamentoRepository.GetRecentAsync(usuarioId, 5, startDate, endDate);

        var response = new DashboardSummaryResponse(
            totalEntradas,
            totalSaidas,
            totalEntradas - totalSaidas,
            catResults.Select(c => new CategoriaGraficoResponse(c.Categoria, c.Valor)),
            trendResults.Select(t => new EvolucaoDiariaResponse(t.Data, t.Entradas, t.Saidas)),
            yearlyResults.Select(y => new EvolucaoMensalGraficoResponse(y.Mes, y.Entradas, y.Saidas, y.Saldo)),
            recentTransactions.Select(l => new LancamentoResponse(
                l.Id, l.Descricao, l.Valor, l.Data, 
                l.Tipo == "Entrada" ? "Receita" : "Despesa",
                l.CategoriaId, l.CategoriaNome))
        );
        
        return Result<DashboardSummaryResponse>.Ok(response);
    }
}
