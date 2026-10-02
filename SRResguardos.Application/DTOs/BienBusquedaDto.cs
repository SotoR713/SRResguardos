namespace SRResguardos.Application.DTOs;

public class BienBusquedaDto
{
    public string Identificador { get; set; } = string.Empty;
    public string TipoBien { get; set; } = string.Empty;
    public string Descripcion { get; set; } = string.Empty;
    public int EstadoId { get; set; }
    public string Estado { get; set; } = string.Empty;
}