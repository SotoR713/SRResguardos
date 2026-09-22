using Microsoft.Data.SqlClient;
using SRResguardos.Application.DTOs;
using SRResguardos.Application.Interfaces.Persistence;
using System.Data;

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

    public async Task<int> AgregarPuestoAsync(string nombre)
    {
        const string sql = @"
        INSERT INTO Puestos (Nombre)
        OUTPUT INSERTED.Id
        VALUES (@Nombre);";

        await using var conexion = new SqlConnection(_cadenaConexion);
        await using var comando = new SqlCommand(sql, conexion);

        comando.Parameters.Add("@Nombre", SqlDbType.VarChar, 50).Value = nombre.Trim();

        await conexion.OpenAsync();

        try
        {
            var id = await comando.ExecuteScalarAsync();
            return (int)id!;
        }
        catch (SqlException ex) when (ex.Number is 2627 or 2601)
        {
            throw new InvalidOperationException($"Ya existe un puesto llamado '{nombre.Trim()}'.");
        }
    }

    public async Task EliminarPuestoAsync(int id)
    {
        const string sql = "DELETE FROM Puestos WHERE Id = @Id;";

        await using var conexion = new SqlConnection(_cadenaConexion);
        await using var comando = new SqlCommand(sql, conexion);

        comando.Parameters.Add("@Id", SqlDbType.Int).Value = id;

        await conexion.OpenAsync();

        try
        {
            var filas = await comando.ExecuteNonQueryAsync();

            if (filas == 0)
                throw new InvalidOperationException("El puesto ya no existe.");
        }
        catch (SqlException ex) when (ex.Number == 547)
        {
            throw new InvalidOperationException(
                "No se puede eliminar: hay empleados o resguardos con este puesto.");
        }
    }

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