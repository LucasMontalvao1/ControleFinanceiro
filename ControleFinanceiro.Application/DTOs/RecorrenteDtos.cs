using System;

namespace ControleFinanceiro.Application.DTOs;

public record RecorrenteRequest(
    int CategoriaId,
    string Descricao,
    decimal Valor,
    int DiaVencimento,
    string Tipo,
    bool Ativo);

public record RecorrenteResponse(
    int Id,
    int CategoriaId,
    string CategoriaNome,
    string Descricao,
    decimal Valor,
    int DiaVencimento,
    string Tipo,
    bool Ativo);
