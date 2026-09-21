namespace SRResguardos.Application.DTOs;

public class ResguardoListaDto
{
    public int Id { get; set; }
    public string NumeroSerie { get; set; } = string.Empty;
    public string Colaborador { get; set; } = string.Empty;
    public string Puesto { get; set; } = string.Empty;
    public string Entrega { get; set; } = string.Empty;
    public string TipoBien { get; set; } = string.Empty;
    public string IdentificadorBien { get; set; } = string.Empty;
    public DateOnly Fecha { get; set; }
    public string EstadoResguardo { get; set; } = string.Empty;
    public string EstadoBien { get; set; } = string.Empty;
}