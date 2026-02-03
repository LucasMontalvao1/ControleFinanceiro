using System;

namespace ControleFinanceiro.Domain.Entities;

public class Meta
{
    public int Id { get; set; }
    public int UsuarioId { get; set; }
    public int CategoriaId { get; set; }
    public string CategoriaNome { get; set; } = string.Empty;
    public decimal ValorLimite { get; set; }
    public int Mes { get; set; }
    public int Ano { get; set; }
}
