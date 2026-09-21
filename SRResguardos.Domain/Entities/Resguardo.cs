namespace SRResguardos.Domain.Entities;

public class Resguardo
{
    public int Id { get; set; }
    public string NumeroSerie { get; set; } = string.Empty;
    public int ColaboradorId { get; set; }
    public int PuestoId { get; set; }
    public int EntregaId { get; set; }
    public string IdentificadorBien { get; set; } = string.Empty;
    public DateOnly Fecha { get; set; }
    public int EstadoId { get; set; }
    public string Notas { get; set; } = string.Empty;
    public DateTime UltimaModificacion { get; set; }
}