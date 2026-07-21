namespace SRResguardos.Domain.Entities;

public class Automovil : Activo
{

    public string MarcaAuto { get; protected set; }

    public string ModeloAuto { get; protected set; }

    public string PlacasAuto { get; protected set; }
}