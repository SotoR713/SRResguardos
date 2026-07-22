namespace SRResguardos.Domain.Entities;

public class Resguardo
{
    public int IdResguardo { get; protected set; }
    public int IdEmpresa { get; protected set; }
    public int IdActivo { get; protected set; }
    public int IdEmpleado { get; protected set; }
    public DateOnly Fecha { get; protected set; }
    public string Observaciones { get; protected set; }
    public int IdEmpleadoEntrega { get; protected set; }
    public int IDEstadoResguardo { get; protected set; }
    public DateOnly FechaFin {  get; protected set; }


}