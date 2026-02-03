using System;

namespace ControleFinanceiro.Domain.Entities;

public class Recorrente
{
    public int Id { get; set; }
    public int UsuarioId { get; set; }
    public int CategoriaId { get; set; }
    public string CategoriaNome { get; set; } = string.Empty;
    public string Descricao { get; set; } = string.Empty;
    public decimal Valor { get; set; }
    public int DiaVencimento { get; set; }
    public string Tipo { get; set; } = string.Empty; // Entrada ou Saida
    public bool Ativo { get; set; } = true;
    public DateTime CreatedAt { get; set; }

    public Recorrente()
    {
        CreatedAt = DateTime.UtcNow;
    }
}
