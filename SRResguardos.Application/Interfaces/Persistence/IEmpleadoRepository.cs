using SRResguardos.Application.DTOs;

namespace SRResguardos.Application.Interfaces.Persistence;

public interface IEmpleadoRepository
{
    Task<IReadOnlyList<EmpleadoListaDto>> ObtenerListaAsync();
    Task<EmpleadoListaDto?> ObtenerPorIdAsync(int id);
    Task<int> CrearAsync(NuevoEmpleadoDto empleado);
    Task ActivarAsync(int id);
    Task InactivarAsync(int id);
}
