namespace SRResguardos.Domain.Entities;

public class Computadora : Activo
{
    public string ModeloComputadora { get; protected set; }

    public string MarcaComputadora { get; protected set; }

    public string DiscoDuroComputadora { get; protected set; }

    public string RAMComputadora { get; protected set; }

    public string ProcesadorComputadora { get; protected set; }

    public string NumeroSerieComputadora { get; protected set; }
}