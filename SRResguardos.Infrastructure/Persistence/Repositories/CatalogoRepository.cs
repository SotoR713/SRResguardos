using Microsoft.Data.SqlClient;
using SRResguardos.Application.DTOs;
using SRResguardos.Application.Interfaces.Persistence;

namespace SRResguardos.Infrastructure.Persistence.Repositories;

public class CatalogoRepository : ICatalogoRepository
{
    private readonly string _cadenaConexion;

    public CatalogoRepository(string cadenaConexion)
    {
        _cadenaConexion = cadenaConexion;
    }

    public Task<IReadOnlyList<CatalogoDto>> ObtenerEstadosResguardoAsync()
        => ConsultarAsync("SELECT Id, Nombre FROM EstadosResguardo ORDER BY Id;");

    public Task<IReadOnlyList<CatalogoDto>> ObtenerPuestosAsync()
        => ConsultarAsync("SELECT Id, Nombre FROM Puestos ORDER BY Nombre;");

    public Task<IReadOnlyList<CatalogoDto>> ObtenerEstatusAsync()
        => ConsultarAsync("SELECT Id, Nombre FROM Estatus ORDER BY Id;");

    private async Task<IReadOnlyList<CatalogoDto>> ConsultarAsync(string sql)
    {
        var lista = new List<CatalogoDto>();

        await using var conexion = new SqlConnection(_cadenaConexion);
        await using var comando = new SqlCommand(sql, conexion);

        await conexion.OpenAsync();

        await using var lector = await comando.ExecuteReaderAsync();

        while (await lector.ReadAsync())
        {
            lista.Add(new CatalogoDto
            {
                Id = lector.GetInt32(0),
                Nombre = lector.GetString(1)
            });
        }

        return lista;
    }
}