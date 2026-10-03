using System.ComponentModel.DataAnnotations;

namespace SRResguardos.Application.DTOs;

public class NuevoFondoFijoDto
{
    // Los límites van en texto; ParseLimitsInInvariantCulture evita que el punto
    // decimal se interprete distinto según la configuración regional del servidor.
    [Range(typeof(decimal), "0.01", "9999999999.99",
        ParseLimitsInInvariantCulture = true,
        ErrorMessage = "El monto debe ser mayor que cero.")]
    public decimal Monto { get; set; }
}
