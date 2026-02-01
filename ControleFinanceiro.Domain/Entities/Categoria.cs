namespace ControleFinanceiro.Domain.Entities;

public class Categoria
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Tipo { get; set; } = string.Empty; // Receita ou Despesa
    public int UsuarioId { get; set; }
    public bool IsDefault { get; set; }
    public DateTime CreatedAt { get; set; }

    public Categoria()
    {
        CreatedAt = DateTime.UtcNow;
    }
}
