namespace SRResguardos.Application.DTOs;

public class ResponsivaDto
{
    public int Id { get; set; }
    public string NumeroSerie { get; set; } = string.Empty;
    public DateOnly Fecha { get; set; }
    public string Colaborador { get; set; } = string.Empty;
    public string Puesto { get; set; } = string.Empty;
    public string Entrega { get; set; } = string.Empty;
    public string TipoBien { get; set; } = string.Empty;
    public string IdentificadorBien { get; set; } = string.Empty;
    public string Notas { get; set; } = string.Empty;
    public string EstadoResguardo { get; set; } = string.Empty;
    public List<CaracteristicaDto> Caracteristicas { get; set; } = [];
}