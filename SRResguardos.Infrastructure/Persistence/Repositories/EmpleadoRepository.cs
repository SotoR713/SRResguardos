using Microsoft.Data.SqlClient;
using SRResguardos.Application.DTOs;
using SRResguardos.Application.Interfaces.Persistence;
using SRResguardos.Domain.Enums;
using System.Data;

namespace SRResguardos.Infrastructure.Persistence.Repositories;

public class EmpleadoRepository : IEmpleadoRepository
{
    private readonly string _cadenaConexion;

    public EmpleadoRepository(string cadenaConexion)
    {
        _cadenaConexion = cadenaConexion;
    }

    public Task<IReadOnlyList<EmpleadoListaDto>> ObtenerListaAsync() => ConsultarAsync(null);

    public async Task<EmpleadoListaDto?> ObtenerPorIdAsync(int id)
    {
        var lista = await ConsultarAsync(id);
        return lista.Count > 0 ? lista[0] : null;
    }

    public async Task<int> CrearAsync(NuevoEmpleadoDto empleado)
    {
        const string sql = @"
        INSERT INTO Empleados (Nombre, PuestoId, EstatusId)
        OUTPUT INSERTED.Id
        VALUES (@Nombre, @PuestoId, @EstatusId);";

        await using var conexion = new SqlConnection(_cadenaConexion);
        await using var comando = new SqlCommand(sql, conexion);

        comando.Parameters.Add("@Nombre", SqlDbType.VarChar, 50).Value = empleado.Nombre.Trim();
        comando.Parameters.Add("@PuestoId", SqlDbType.Int).Value = empleado.PuestoId;
        comando.Parameters.Add("@EstatusId", SqlDbType.Int).Value = (int)EstatusEmpleado.Activo;

        await conexion.OpenAsync();

        try
        {
            var id = await comando.ExecuteScalarAsync();
            return (int)id!;
        }
        catch (SqlException ex) when (ex.Number == 547)
        {
            throw new InvalidOperationException("El puesto elegido ya no existe.");
        }
    }

    public async Task ActivarAsync(int id)
    {
        const string sql = @"
        UPDATE Empleados
        SET EstatusId = @Activo
        WHERE Id = @Id;";

        await using var conexion = new SqlConnection(_cadenaConexion);
        await using var comando = new SqlCommand(sql, conexion);

        comando.Parameters.Add("@Id", SqlDbType.Int).Value = id;
        comando.Parameters.Add("@Activo", SqlDbType.Int).Value = (int)EstatusEmpleado.Activo;

        await conexion.OpenAsync();

        var filas = await comando.ExecuteNonQueryAsync();

        if (filas == 0)
            throw new InvalidOperationException("El empleado ya no existe.");
    }

    // La regla vive en el WHERE: solo se inactiva si no tiene resguardos activos.
    // Verificar y cambiar en una sola instrucción evita que alguien le asigne
    // un bien justo entre la revisión y el cambio.
    public async Task InactivarAsync(int id)
    {
        const string sql = @"
        UPDATE Empleados
        SET EstatusId = @Inactivo
        WHERE Id = @Id
          AND NOT EXISTS (SELECT 1 FROM Resguardos
                          WHERE ColaboradorId = @Id
                            AND EstadoId = @ResguardoActivo);";

        const string sqlContar = @"
        SELECT COUNT(*) FROM Resguardos
        WHERE ColaboradorId = @Id
          AND EstadoId = @ResguardoActivo;";

        await using var conexion = new SqlConnection(_cadenaConexion);
        await conexion.OpenAsync();

        await using var comando = new SqlCommand(sql, conexion);
        comando.Parameters.Add("@Id", SqlDbType.Int).Value = id;
        comando.Parameters.Add("@Inactivo", SqlDbType.Int).Value = (int)EstatusEmpleado.Inactivo;
        comando.Parameters.Add("@ResguardoActivo", SqlDbType.Int).Value = (int)EstadoResguardo.Activo;

        var filas = await comando.ExecuteNonQueryAsync();

        if (filas > 0)
            return;

        // No se cambió nada: averiguar por qué, solo para dar un mensaje claro.
        await using var cmdContar = new SqlCommand(sqlContar, conexion);
        cmdContar.Parameters.Add("@Id", SqlDbType.Int).Value = id;
        cmdContar.Parameters.Add("@ResguardoActivo", SqlDbType.Int).Value = (int)EstadoResguardo.Activo;

        var activos = (int)(await cmdContar.ExecuteScalarAsync())!;

        throw new InvalidOperationException(activos > 0
            ? $"No se puede inactivar: tiene {activos} resguardo(s) activo(s). Primero deben devolverse."
            : "El empleado ya no existe.");
    }

    // Una sola consulta para la lista y para un empleado.
    // El LEFT JOIN con la condición del estado en el ON (y no en el WHERE) es lo que
    // conserva a los empleados que no tienen ningún resguardo activo: les cuenta cero.
    private async Task<IReadOnlyList<EmpleadoListaDto>> ConsultarAsync(int? id)
    {
        const string sql = @"
            SELECT
                e.Id,
                e.Nombre,
                p.Nombre AS Puesto,
                s.Nombre AS Estatus,
                e.EstatusId,
                COUNT(r.Id) AS ResguardosActivos
            FROM Empleados e
            INNER JOIN Puestos p ON p.Id = e.PuestoId
            INNER JOIN Estatus s ON s.Id = e.EstatusId
            LEFT JOIN Resguardos r ON r.ColaboradorId = e.Id
                                  AND r.EstadoId = @ResguardoActivo
            WHERE (@Id IS NULL OR e.Id = @Id)
            GROUP BY e.Id, e.Nombre, p.Nombre, s.Nombre, e.EstatusId
            ORDER BY e.Nombre;";

        var lista = new List<EmpleadoListaDto>();

        await using var conexion = new SqlConnection(_cadenaConexion);
        await using var comando = new SqlCommand(sql, conexion);

        comando.Parameters.Add("@Id", SqlDbType.Int).Value = (object?)id ?? DBNull.Value;
        comando.Parameters.Add("@ResguardoActivo", SqlDbType.Int).Value = (int)EstadoResguardo.Activo;

        await conexion.OpenAsync();

        await using var lector = await comando.ExecuteReaderAsync();

        while (await lector.ReadAsync())
        {
            lista.Add(new EmpleadoListaDto
            {
                Id = lector.GetInt32(0),
                Nombre = lector.GetString(1),
                Puesto = lector.GetString(2),
                Estatus = lector.GetString(3),
                EstatusId = lector.GetInt32(4),
                ResguardosActivos = lector.GetInt32(5)
            });
        }

        return lista;
    }
}
