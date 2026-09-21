using SRResguardos.Application.DTOs;

namespace SRResguardos.Application.Interfaces.Persistence;

public interface IResguardoRepository
{
    Task<IReadOnlyList<ResguardoListaDto>> ObtenerListaAsync(int? estadoId = null);
}