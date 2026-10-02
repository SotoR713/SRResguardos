using SRResguardos.Application.DTOs;

namespace SRResguardos.Application.Interfaces.Persistence;

public interface IBienRepository
{
    Task<IReadOnlyList<BienBusquedaDto>> ObtenerParaBusquedaAsync();
}