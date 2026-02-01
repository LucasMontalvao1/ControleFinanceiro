namespace ControleFinanceiro.Domain.Entities;

public class Lancamento
{
    public int Id { get; set; }
    public string Descricao { get; set; } = string.Empty;
    public decimal Valor { get; set; }
    public DateTime Data { get; set; }
    public string Tipo { get; set; } = string.Empty; // Entrada ou Saida
    public int UsuarioId { get; set; }
    public int CategoriaId { get; set; }
    public DateTime CreatedAt { get; set; }

    public Lancamento()
    {
        CreatedAt = DateTime.UtcNow;
        Data = DateTime.UtcNow;
    }
}
