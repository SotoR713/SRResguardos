namespace SRResguardos.Domain.Entities;

public class Empleado
{
    public int IdEmpleado { get; protected set; }

    public string NombreEmpleado { get; protected set; }

    public int IdDepartamento { get; protected set; }
    public int IdPuesto { get; protected set; }

}