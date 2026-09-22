using Microsoft.Data.SqlClient;
using SRResguardos.Application.DTOs;
using SRResguardos.Application.Interfaces.Persistence;

namespace SRResguardos.Infrastructure.Persistence.Repositories;

public class ResguardoRepository : IResguardoRepository
{
    private readonly string _cadenaConexion;

    public ResguardoRepository(string cadenaConexion)
    {
        _cadenaConexion = cadenaConexion;
    }

    public async Task<IReadOnlyList<ResguardoListaDto>> ObtenerListaAsync(int? estadoId = null)
    {
        const string sql = @"
        SELECT
            r.Id,
            r.NumeroSerie,
            e.Nombre  AS Colaborador,
            p.Nombre  AS Puesto,
            en.Nombre AS Entrega,
            t.Nombre  AS TipoBien,
            r.IdentificadorBien,
            r.Fecha,
            er.Nombre AS EstadoResguardo,
            eb.Nombre AS EstadoBien
        FROM Resguardos r
        INNER JOIN Empleados e         ON e.Id  = r.ColaboradorId
        INNER JOIN Empleados en        ON en.Id = r.EntregaId
        INNER JOIN Puestos p           ON p.Id  = r.PuestoId
        INNER JOIN Bienes b            ON b.Identificador = r.IdentificadorBien
        INNER JOIN TiposBien t         ON t.Id  = b.TipoBienId
        INNER JOIN EstadosBienes eb    ON eb.Id = b.EstadoId
        INNER JOIN EstadosResguardo er ON er.Id = r.EstadoId
        WHERE (@EstadoId IS NULL OR r.EstadoId = @EstadoId)
        ORDER BY r.NumeroSerie;";

        var lista = new List<ResguardoListaDto>();

        await using var conexion = new SqlConnection(_cadenaConexion);
        await using var comando = new SqlCommand(sql, conexion);

        comando.Parameters.Add("@EstadoId", System.Data.SqlDbType.Int).Value =
            (object?)estadoId ?? DBNull.Value;

        await conexion.OpenAsync();

        await using var lector = await comando.ExecuteReaderAsync();

        while (await lector.ReadAsync())
        {
            lista.Add(new ResguardoListaDto
            {
                Id = lector.GetInt32(0),
                NumeroSerie = lector.GetString(1),
                Colaborador = lector.GetString(2),
                Puesto = lector.GetString(3),
                Entrega = lector.GetString(4),
                TipoBien = lector.GetString(5),
                IdentificadorBien = lector.GetString(6),
                Fecha = DateOnly.FromDateTime(lector.GetDateTime(7)),
                EstadoResguardo = lector.GetString(8),
                EstadoBien = lector.GetString(9)
            });
        }

        return lista;
    }
}
