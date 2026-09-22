namespace SRResguardos.Application.DTOs;

public class EmpleadoListaDto
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Puesto { get; set; } = string.Empty;
    public string Estatus { get; set; } = string.Empty;
}