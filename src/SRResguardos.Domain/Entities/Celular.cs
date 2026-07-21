namespace SRResguardos.Domain.Entities;

public class Celular : Activo
{
    public string IMEI { get; protected set; }

    public string NumeroCelular { get; protected set; }

    public string MemoriaCelular { get; protected set; }

    public string MarcaCelular { get; protected set; }

    public string ModeloCelular { get; protected set; }
}