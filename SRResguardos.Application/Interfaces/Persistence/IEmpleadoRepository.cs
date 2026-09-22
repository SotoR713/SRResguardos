using SRResguardos.Application.DTOs;

namespace SRResguardos.Application.Interfaces.Persistence;

public interface IEmpleadoRepository
{
    Task<IReadOnlyList<EmpleadoListaDto>> ObtenerListaAsync();
}