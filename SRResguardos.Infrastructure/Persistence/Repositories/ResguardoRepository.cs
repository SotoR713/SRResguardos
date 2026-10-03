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

    public async Task<IReadOnlyList<ResguardoListaDto>> ObtenerListaAsync(int? estadoId = null, int? colaboradorId = null)
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
          AND (@ColaboradorId IS NULL OR r.ColaboradorId = @ColaboradorId)
        ORDER BY r.NumeroSerie;";

        var lista = new List<ResguardoListaDto>();

        await using var conexion = new SqlConnection(_cadenaConexion);
        await using var comando = new SqlCommand(sql, conexion);

        comando.Parameters.Add("@EstadoId", SqlDbType.Int).Value = (object?)estadoId ?? DBNull.Value;
        comando.Parameters.Add("@ColaboradorId", SqlDbType.Int).Value = (object?)colaboradorId ?? DBNull.Value;

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
                EstadoBien = lector.GetString(9),
                EstadoResguardoId = lector.GetInt32(10)
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
            er.Nombre AS EstadoResguardo,
            r.EstadoId
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

        comando.Parameters.Add("@Id", SqlDbType.Int).Value = id;

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
            EstadoResguardo = lector.GetString(9),
            EstadoResguardoId = lector.GetInt32(10)
        };

        await lector.NextResultAsync();

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

    // ------------------------------------------------------------------
    // ASIGNAR: tres pasos que van juntos en una transacción.
    //   1. Reservar el bien (Disponible -> Asignado). Si no estaba disponible, no procede.
    //   2. Insertar el resguardo con folio de la secuencia y el puesto congelado.
    //   3. Congelar las características del bien según su tipo.
    // ------------------------------------------------------------------
    public async Task<int> AsignarAsync(NuevoResguardoDto nuevo)
    {
        const string sqlReservarBien = @"
            UPDATE Bienes
            SET EstadoId = @Asignado,
                UltimaModificacion = SYSDATETIME()
            OUTPUT INSERTED.TipoBienId, INSERTED.BienId
            WHERE Identificador = @Identificador
              AND EstadoId = @Disponible;";

        // INSERT ... SELECT: el puesto se toma del empleado en este instante (queda congelado),
        // y el WHERE impide asignar si el colaborador o quien entrega no están activos.
        const string sqlInsertarResguardo = @"
            DECLARE @Folio INT;
            SET @Folio = NEXT VALUE FOR dbo.FolioResguardo;

            INSERT INTO Resguardos
                (NumeroSerie, ColaboradorId, PuestoId, EntregaId, IdentificadorBien, Fecha, EstadoId, Notas)
            OUTPUT INSERTED.Id
            SELECT
                CONCAT('RES-', FORMAT(@Folio, '0000')),
                e.Id,
                e.PuestoId,
                @EntregaId,
                @Identificador,
                @Fecha,
                @ResguardoActivo,
                @Notas
            FROM Empleados e
            WHERE e.Id = @ColaboradorId
              AND e.EstatusId = @EmpleadoActivo
              AND EXISTS (SELECT 1 FROM Empleados x
                          WHERE x.Id = @EntregaId
                            AND x.EstatusId = @EmpleadoActivo);";

        await using var conexion = new SqlConnection(_cadenaConexion);
        await conexion.OpenAsync();

        await using var transaccion = (SqlTransaction)await conexion.BeginTransactionAsync();

        try
        {
            // 1. Reservar el bien
            int tipoBienId;
            int bienId;

            await using (var cmdBien = new SqlCommand(sqlReservarBien, conexion, transaccion))
            {
                cmdBien.Parameters.Add("@Identificador", SqlDbType.VarChar, 20).Value = nuevo.IdentificadorBien.Trim();
                cmdBien.Parameters.Add("@Asignado", SqlDbType.Int).Value = (int)EstadoBien.Asignado;
                cmdBien.Parameters.Add("@Disponible", SqlDbType.Int).Value = (int)EstadoBien.Disponible;

                // El lector se cierra al salir de este bloque; hay que cerrarlo
                // antes de mandar el siguiente comando por la misma conexión.
                await using var lector = await cmdBien.ExecuteReaderAsync();

                if (!await lector.ReadAsync())
                    throw new InvalidOperationException("El bien debe estar disponible.");

                tipoBienId = lector.GetInt32(0);
                bienId = lector.GetInt32(1);
            }

            // 2. Insertar el resguardo
            int resguardoId;

            await using (var cmdResguardo = new SqlCommand(sqlInsertarResguardo, conexion, transaccion))
            {
                cmdResguardo.Parameters.Add("@ColaboradorId", SqlDbType.Int).Value = nuevo.ColaboradorId;
                cmdResguardo.Parameters.Add("@EntregaId", SqlDbType.Int).Value = nuevo.EntregaId;
                cmdResguardo.Parameters.Add("@Identificador", SqlDbType.VarChar, 20).Value = nuevo.IdentificadorBien.Trim();
                cmdResguardo.Parameters.Add("@Fecha", SqlDbType.Date).Value = nuevo.Fecha.ToDateTime(TimeOnly.MinValue);
                cmdResguardo.Parameters.Add("@Notas", SqlDbType.VarChar, 500).Value = nuevo.Notas.Trim();
                cmdResguardo.Parameters.Add("@ResguardoActivo", SqlDbType.Int).Value = (int)EstadoResguardo.Activo;
                cmdResguardo.Parameters.Add("@EmpleadoActivo", SqlDbType.Int).Value = (int)EstatusEmpleado.Activo;

                var resultado = await cmdResguardo.ExecuteScalarAsync();

                if (resultado is null)
                    throw new InvalidOperationException("El colaborador y quien entrega deben estar activos.");

                resguardoId = (int)resultado;
            }

            // 3. Congelar las características según el tipo de bien
            var sqlCaracteristicas = (TipoBien)tipoBienId switch
            {
                TipoBien.Vehiculo => SqlCaracteristicasVehiculo,
                TipoBien.Celular => SqlCaracteristicasCelular,
                TipoBien.Laptop => SqlCaracteristicasLaptop,
                TipoBien.FondoFijo => SqlCaracteristicasFondoFijo,
                TipoBien.Otros => SqlCaracteristicasOtros,
                _ => throw new InvalidOperationException("Tipo de bien desconocido.")
            };

            await using (var cmdCaracteristicas = new SqlCommand(sqlCaracteristicas, conexion, transaccion))
            {
                cmdCaracteristicas.Parameters.Add("@ResguardoId", SqlDbType.Int).Value = resguardoId;
                cmdCaracteristicas.Parameters.Add("@BienId", SqlDbType.Int).Value = bienId;

                await cmdCaracteristicas.ExecuteNonQueryAsync();
            }

            await transaccion.CommitAsync();

            return resguardoId;
        }
        catch (SqlException ex) when (ex.Number == 547 && ex.Message.Contains("CK_Resguardos_EntregaDistinta"))
        {
            await transaccion.RollbackAsync();
            throw new InvalidOperationException("Quien entrega debe ser una persona distinta del colaborador.");
        }
        catch (SqlException ex) when ((ex.Number is 2601 or 2627) && ex.Message.Contains("UX_Resguardos_BienActivo"))
        {
            await transaccion.RollbackAsync();
            throw new InvalidOperationException("Ese bien ya tiene un resguardo activo.");
        }
        catch
        {
            await transaccion.RollbackAsync();
            throw;
        }
    }

    public Task DevolverAsync(int id) => CerrarAsync(id, EstadoResguardo.Devuelto);

    public Task CancelarAsync(int id) => CerrarAsync(id, EstadoResguardo.Cancelado);

    // Devolver y cancelar hacen lo mismo con distinto estado final:
    // el resguardo deja de estar activo y el bien, si estaba asignado, queda disponible.
    // Si el bien estaba en mantenimiento, se queda en mantenimiento.
    private async Task CerrarAsync(int id, EstadoResguardo nuevoEstado)
    {
        const string sqlResguardo = @"
        UPDATE Resguardos
        SET EstadoId = @NuevoEstado,
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
            cmdResguardo.Parameters.Add("@NuevoEstado", SqlDbType.Int).Value = (int)nuevoEstado;

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

    // ------------------------------------------------------------------
    // Características congeladas por tipo.
    // CROSS APPLY (VALUES ...) convierte columnas en renglones etiqueta-valor.
    // Se omiten los valores nulos, y el ORDER BY garantiza que los Id de
    // Caracteristicas sigan el orden en que deben imprimirse.
    // ------------------------------------------------------------------
    private const string SqlCaracteristicasVehiculo = @"
        INSERT INTO Caracteristicas (ResguardoId, Etiqueta, Valor)
        SELECT @ResguardoId, c.Etiqueta, c.Valor
        FROM Vehiculos v
        CROSS APPLY (VALUES
            (1, N'Nombre', v.Nombre),
            (2, N'Marca', v.Marca),
            (3, N'Modelo', v.Modelo),
            (4, N'Número de serie', v.NumeroSerie),
            (5, N'Seguro', v.Seguro),
            (6, N'Información adicional', v.Caracteristica)
        ) AS c (Orden, Etiqueta, Valor)
        WHERE v.Id = @BienId
          AND c.Valor IS NOT NULL
        ORDER BY c.Orden;";

    private const string SqlCaracteristicasCelular = @"
        INSERT INTO Caracteristicas (ResguardoId, Etiqueta, Valor)
        SELECT @ResguardoId, c.Etiqueta, c.Valor
        FROM Celulares cel
        CROSS APPLY (VALUES
            (1, N'Marca', cel.Marca),
            (2, N'Modelo', cel.Modelo),
            (3, N'IMEI', cel.Imei),
            (4, N'Número', cel.Numero),
            (5, N'RAM', cel.Ram),
            (6, N'Memoria', cel.Memoria),
            (7, N'Información adicional', cel.Caracteristica)
        ) AS c (Orden, Etiqueta, Valor)
        WHERE cel.Id = @BienId
          AND c.Valor IS NOT NULL
        ORDER BY c.Orden;";

    private const string SqlCaracteristicasLaptop = @"
        INSERT INTO Caracteristicas (ResguardoId, Etiqueta, Valor)
        SELECT @ResguardoId, c.Etiqueta, c.Valor
        FROM Laptops l
        CROSS APPLY (VALUES
            (1, N'Marca', l.Marca),
            (2, N'Modelo', l.Modelo),
            (3, N'Número de serie', l.Serie),
            (4, N'RAM', l.Ram),
            (5, N'Disco duro', l.DiscoDuro),
            (6, N'Procesador', l.Procesador),
            (7, N'Información adicional', l.Caracteristica)
        ) AS c (Orden, Etiqueta, Valor)
        WHERE l.Id = @BienId
          AND c.Valor IS NOT NULL
        ORDER BY c.Orden;";

    private const string SqlCaracteristicasFondoFijo = @"
        INSERT INTO Caracteristicas (ResguardoId, Etiqueta, Valor)
        SELECT @ResguardoId, N'Monto', FORMAT(f.Monto, 'C', 'es-MX')
        FROM FondosFijos f
        WHERE f.Id = @BienId;";

    private const string SqlCaracteristicasOtros = @"
        INSERT INTO Caracteristicas (ResguardoId, Etiqueta, Valor)
        SELECT @ResguardoId, c.Etiqueta, c.Valor
        FROM OtrosBienes o
        CROSS APPLY (VALUES
            (1, N'Descripción', o.Caracteristica1),
            (2, N'Detalle', o.Caracteristica2),
            (3, N'Detalle adicional', o.Caracteristica3)
        ) AS c (Orden, Etiqueta, Valor)
        WHERE o.Id = @BienId
          AND c.Valor IS NOT NULL
        ORDER BY c.Orden;";
}
