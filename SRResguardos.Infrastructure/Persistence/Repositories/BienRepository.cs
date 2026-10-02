using System.Data;
using Microsoft.Data.SqlClient;
using SRResguardos.Application.DTOs;
using SRResguardos.Application.Interfaces.Persistence;
using SRResguardos.Domain.Enums;

namespace SRResguardos.Infrastructure.Persistence.Repositories;

public class BienRepository : IBienRepository
{
    private readonly string _cadenaConexion;

    public BienRepository(string cadenaConexion)
    {
        _cadenaConexion = cadenaConexion;
    }

    public async Task<IReadOnlyList<BienBusquedaDto>> ObtenerParaBusquedaAsync()
    {
        const string sql = @"
            SELECT
                b.Identificador,
                t.Nombre AS TipoBien,
                COALESCE(
                    v.Marca + ' ' + v.Modelo,
                    c.Marca + ' ' + c.Modelo,
                    l.Marca + ' ' + l.Modelo,
                    'Fondo fijo de ' + FORMAT(f.Monto, 'C', 'es-MX'),
                    o.Caracteristica1
                ) AS Descripcion,
                b.EstadoId,
                eb.Nombre AS Estado
            FROM Bienes b
            INNER JOIN TiposBien t      ON t.Id  = b.TipoBienId
            INNER JOIN EstadosBienes eb ON eb.Id = b.EstadoId
            LEFT JOIN Vehiculos v       ON b.TipoBienId = @Vehiculo  AND v.Id = b.BienId
            LEFT JOIN Celulares c       ON b.TipoBienId = @Celular   AND c.Id = b.BienId
            LEFT JOIN Laptops l         ON b.TipoBienId = @Laptop    AND l.Id = b.BienId
            LEFT JOIN FondosFijos f     ON b.TipoBienId = @FondoFijo AND f.Id = b.BienId
            LEFT JOIN OtrosBienes o     ON b.TipoBienId = @Otros     AND o.Id = b.BienId
            ORDER BY b.Identificador;";

        var lista = new List<BienBusquedaDto>();

        await using var conexion = new SqlConnection(_cadenaConexion);
        await using var comando = new SqlCommand(sql, conexion);

        comando.Parameters.Add("@Vehiculo", SqlDbType.Int).Value = (int)TipoBien.Vehiculo;
        comando.Parameters.Add("@Celular", SqlDbType.Int).Value = (int)TipoBien.Celular;
        comando.Parameters.Add("@Laptop", SqlDbType.Int).Value = (int)TipoBien.Laptop;
        comando.Parameters.Add("@FondoFijo", SqlDbType.Int).Value = (int)TipoBien.FondoFijo;
        comando.Parameters.Add("@Otros", SqlDbType.Int).Value = (int)TipoBien.Otros;

        await conexion.OpenAsync();

        await using var lector = await comando.ExecuteReaderAsync();

        while (await lector.ReadAsync())
        {
            lista.Add(new BienBusquedaDto
            {
                Identificador = lector.GetString(0),
                TipoBien = lector.GetString(1),
                Descripcion = lector.IsDBNull(2) ? "(sin datos del tipo)" : lector.GetString(2),
                EstadoId = lector.GetInt32(3),
                Estado = lector.GetString(4)
            });
        }

        return lista;
    }
}