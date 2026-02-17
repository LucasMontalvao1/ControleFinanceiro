using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ControleFinanceiro.Domain.Entities;

[Table("AiScanHistory")]
public class AiScanHistory
{
    [Key]
    public int Id { get; set; }

    [Required]
    [MaxLength(50)]
    public string CorrelationId { get; set; } = string.Empty;

    public int UsuarioId { get; set; }

    [Required]
    [MaxLength(20)]
    public string Status { get; set; } = string.Empty; // "Success", "Failed"

    public long LatencyMs { get; set; }

    [Column(TypeName = "TEXT")] // Store large JSON
    public string? RawJson { get; set; }

    public string? ParseError { get; set; }

    public DateTime ProcessadoEm { get; set; } = DateTime.UtcNow;
}
