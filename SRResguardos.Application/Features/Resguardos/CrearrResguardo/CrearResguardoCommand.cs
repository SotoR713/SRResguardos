namespace SRResguardos.Application.Features.Resguardos.CrearResguardo;

public class CrearResguardoCommand
{
    public int IdEmpresa { get; set; }

    public int IdActivo { get; set; }

    public int IdEmpleado { get; set; }

    public int IdEmpleadoEntrega { get; set; }

    public string Observaciones { get; set; } 
}