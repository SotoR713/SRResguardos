namespace SRResguardos.Domain.Entities;

public class Activo
{
	public int IdActivo { get; protected set; }

	public string CodigoActivo { get; protected set; }

	public string NombreActivo { get; protected set; }

	public EstadoActivo Estado { get; protected set; }

	public int IdEmpresa { get; protected set; }
}