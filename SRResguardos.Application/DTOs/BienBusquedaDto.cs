namespace SRResguardos.Application.DTOs;

public class BienBusquedaDto
{
    public string Identificador { get; set; } = string.Empty;
    public string TipoBien { get; set; } = string.Empty;
    public string Descripcion { get; set; } = string.Empty;
    public int EstadoId { get; set; }
    public string Estado { get; set; } = string.Empty;

    // Datos para reconocer el equipo físico. Pueden venir vacíos según el tipo:
    // un fondo fijo no tiene serie, y solo los celulares tienen teléfono.
    public int TipoBienId { get; set; }
    public string? Marca { get; set; }
    public string? Modelo { get; set; }
    public string? Serie { get; set; }      // número de serie, o IMEI en celulares
    public string? Telefono { get; set; }
}