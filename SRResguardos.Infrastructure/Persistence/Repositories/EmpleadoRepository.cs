using Microsoft.Data.SqlClient;
using SRResguardos.Application.DTOs;
using SRResguardos.Application.Interfaces.Persistence;

namespace SRResguardos.Infrastructure.Persistence.Repositories;

public class EmpleadoRepository : IEmpleadoRepository
{
    private readonly string _cadenaConexion;

    public EmpleadoRepository(string cadenaConexion)
    {
        _cadenaConexion = cadenaConexion;
    }

    public async Task<IReadOnlyList<EmpleadoListaDto>> ObtenerListaAsync()
    {
        const string sql = @"
            SELECT e.Id, e.Nombre, p.Nombre AS Puesto, s.Nombre AS Estatus
            FROM Empleados e
            INNER JOIN Puestos p ON p.Id = e.PuestoId
            INNER JOIN Estatus s ON s.Id = e.EstatusId
            ORDER BY e.Nombre;";

        var lista = new List<EmpleadoListaDto>();

        await using var conexion = new SqlConnection(_cadenaConexion);
        await using var comando = new SqlCommand(sql, conexion);

        await conexion.OpenAsync();

        await using var lector = await comando.ExecuteReaderAsync();

        while (await lector.ReadAsync())
        {
            lista.Add(new EmpleadoListaDto
            {
                Id = lector.GetInt32(0),
                Nombre = lector.GetString(1),
                Puesto = lector.GetString(2),
                Estatus = lector.GetString(3)
            });
        }

        return lista;
    }
}