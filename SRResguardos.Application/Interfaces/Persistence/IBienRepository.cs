using SRResguardos.Application.DTOs;

namespace SRResguardos.Application.Interfaces.Persistence;

public interface IBienRepository
{
    Task<IReadOnlyList<BienBusquedaDto>> ObtenerParaBusquedaAsync();

    // Cada alta devuelve el identificador generado, por ejemplo "3-4".
    Task<string> CrearVehiculoAsync(NuevoVehiculoDto vehiculo);
    Task<string> CrearCelularAsync(NuevoCelularDto celular);
    Task<string> CrearLaptopAsync(NuevoLaptopDto laptop);
    Task<string> CrearFondoFijoAsync(NuevoFondoFijoDto fondo);
    Task<string> CrearOtroAsync(NuevoOtroBienDto otro);

    Task EnviarAMantenimientoAsync(string identificador);
    Task TerminarMantenimientoAsync(string identificador);
    Task DarDeBajaAsync(string identificador);
}
