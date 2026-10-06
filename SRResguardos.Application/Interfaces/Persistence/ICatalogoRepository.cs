using SRResguardos.Application.DTOs;

namespace SRResguardos.Application.Interfaces.Persistence;

public interface ICatalogoRepository
{
    Task<IReadOnlyList<CatalogoDto>> ObtenerEstadosResguardoAsync();
    Task<IReadOnlyList<CatalogoDto>> ObtenerEstadosBienesAsync();
    Task<IReadOnlyList<CatalogoDto>> ObtenerPuestosAsync();
    Task<IReadOnlyList<CatalogoDto>> ObtenerEstatusAsync();
    Task<IReadOnlyList<CatalogoDto>> ObtenerTiposBienAsync();
    Task<int> AgregarPuestoAsync(string nombre);
    Task EliminarPuestoAsync(int id);
}
