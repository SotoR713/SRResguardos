using SRResguardos.Application.DTOs;

namespace SRResguardos.Application.Interfaces.Persistence;

public interface IResguardoRepository
{
    Task<IReadOnlyList<ResguardoListaDto>> ObtenerListaAsync(int? estadoId = null, int? colaboradorId = null);
    Task<ResponsivaDto?> ObtenerResponsivaAsync(int id);
    Task<int> AsignarAsync(NuevoResguardoDto nuevo);
    Task DevolverAsync(int id);
    Task CancelarAsync(int id);
}
