using Microsoft.Data.SqlClient;
using SRResguardos.Application.DTOs;
using SRResguardos.Application.Interfaces.Persistence;
using SRResguardos.Domain.Enums;
using System.Data;

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
            eb.Nombre AS EstadoBien,
            r.EstadoId
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



    public async Task<ResponsivaDto?> ObtenerResponsivaAsync(int id)
    {
        const string sql = @"
        SELECT
            r.Id,
            r.NumeroSerie,
            r.Fecha,
            e.Nombre  AS Colaborador,
            p.Nombre  AS Puesto,
            en.Nombre AS Entrega,
            t.Nombre  AS TipoBien,
            r.IdentificadorBien,
            r.Notas,
            er.Nombre AS EstadoResguardo
        FROM Resguardos r
        INNER JOIN Empleados e         ON e.Id  = r.ColaboradorId
        INNER JOIN Empleados en        ON en.Id = r.EntregaId
        INNER JOIN Puestos p           ON p.Id  = r.PuestoId
        INNER JOIN Bienes b            ON b.Identificador = r.IdentificadorBien
        INNER JOIN TiposBien t         ON t.Id  = b.TipoBienId
        INNER JOIN EstadosResguardo er ON er.Id = r.EstadoId
        WHERE r.Id = @Id;

        SELECT Etiqueta, Valor
        FROM Caracteristicas
        WHERE ResguardoId = @Id
        ORDER BY Id;";

        await using var conexion = new SqlConnection(_cadenaConexion);
        await using var comando = new SqlCommand(sql, conexion);

        comando.Parameters.Add("@Id", System.Data.SqlDbType.Int).Value = id;

        await conexion.OpenAsync();

        await using var lector = await comando.ExecuteReaderAsync();

        if (!await lector.ReadAsync())
            return null;

        var responsiva = new ResponsivaDto
        {
            Id = lector.GetInt32(0),
            NumeroSerie = lector.GetString(1),
            Fecha = DateOnly.FromDateTime(lector.GetDateTime(2)),
            Colaborador = lector.GetString(3),
            Puesto = lector.GetString(4),
            Entrega = lector.GetString(5),
            TipoBien = lector.GetString(6),
            IdentificadorBien = lector.GetString(7),
            Notas = lector.GetString(8),
            EstadoResguardo = lector.GetString(9)
        };

        await lector.NextResultAsync();
        EstadoResguardoId = lector.GetInt32(10)

        while (await lector.ReadAsync())
        {
            responsiva.Caracteristicas.Add(new CaracteristicaDto
            {
                Etiqueta = lector.GetString(0),
                Valor = lector.GetString(1)
            });
        }

        return responsiva;
    }

    public async Task DevolverAsync(int id)
    {
        const string sqlResguardo = @"
        UPDATE Resguardos
        SET EstadoId = @Devuelto,
            UltimaModificacion = SYSDATETIME()
        OUTPUT INSERTED.IdentificadorBien
        WHERE Id = @Id
          AND EstadoId = @Activo;";

        const string sqlBien = @"
        UPDATE Bienes
        SET EstadoId = @Disponible,
            UltimaModificacion = SYSDATETIME()
        WHERE Identificador = @Identificador
          AND EstadoId = @Asignado;";

        await using var conexion = new SqlConnection(_cadenaConexion);
        await conexion.OpenAsync();

        await using var transaccion = (SqlTransaction)await conexion.BeginTransactionAsync();

        try
        {
            await using var cmdResguardo = new SqlCommand(sqlResguardo, conexion, transaccion);
            cmdResguardo.Parameters.Add("@Id", SqlDbType.Int).Value = id;
            cmdResguardo.Parameters.Add("@Activo", SqlDbType.Int).Value = (int)EstadoResguardo.Activo;
            cmdResguardo.Parameters.Add("@Devuelto", SqlDbType.Int).Value = (int)EstadoResguardo.Devuelto;

            var identificador = (string?)await cmdResguardo.ExecuteScalarAsync();

            if (identificador is null)
                throw new InvalidOperationException("Este resguardo ya no está activo.");

            await using var cmdBien = new SqlCommand(sqlBien, conexion, transaccion);
            cmdBien.Parameters.Add("@Identificador", SqlDbType.VarChar, 20).Value = identificador;
            cmdBien.Parameters.Add("@Asignado", SqlDbType.Int).Value = (int)EstadoBien.Asignado;
            cmdBien.Parameters.Add("@Disponible", SqlDbType.Int).Value = (int)EstadoBien.Disponible;

            await cmdBien.ExecuteNonQueryAsync();

            await transaccion.CommitAsync();
        }
        catch
        {
            await transaccion.RollbackAsync();
            throw;
        }
    }
}