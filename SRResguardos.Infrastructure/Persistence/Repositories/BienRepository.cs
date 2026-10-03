using Microsoft.Data.SqlClient;
using SRResguardos.Application.DTOs;
using SRResguardos.Application.Interfaces.Persistence;
using SRResguardos.Domain.Enums;
using System.Data;

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

    // ------------------------------------------------------------------
    // ALTA DE BIENES
    // Cada tabla de tipo tiene Identificador NOT NULL UNIQUE, pero el número
    // del identificador ("3-4") sale del Id que todavía no existe al insertar.
    // Solución sin cambiar el esquema, todo dentro de una transacción:
    //   1. Insertar con un identificador provisional único por sesión (TMP-<sesión>).
    //   2. Leer el Id que generó el IDENTITY con SCOPE_IDENTITY().
    //   3. Reemplazar el provisional por el definitivo: tipo-Id.
    //   4. Registrar el bien en la tabla global Bienes como Disponible.
    // Nadie fuera de la transacción llega a ver el valor provisional.
    // ------------------------------------------------------------------
    public Task<string> CrearVehiculoAsync(NuevoVehiculoDto vehiculo) => CrearAsync(
        TipoBien.Vehiculo,
        @"
        INSERT INTO Vehiculos (Identificador, NumeroSerie, Seguro, Modelo, Marca, Nombre, Caracteristica)
        VALUES (CONCAT('TMP-', @@SPID), @NumeroSerie, @Seguro, @Modelo, @Marca, @Nombre, @Caracteristica);

        DECLARE @Id INT = SCOPE_IDENTITY();
        DECLARE @Identificador VARCHAR(20) = CONCAT(@TipoBienId, '-', @Id);

        UPDATE Vehiculos SET Identificador = @Identificador WHERE Id = @Id;

        INSERT INTO Bienes (TipoBienId, BienId, Identificador, EstadoId)
        VALUES (@TipoBienId, @Id, @Identificador, @Disponible);

        SELECT @Identificador;",
        "Ya existe un vehículo con ese número de serie.",
        p =>
        {
            p.Add("@NumeroSerie", SqlDbType.VarChar, 50).Value = Texto(vehiculo.NumeroSerie);
            p.Add("@Seguro", SqlDbType.VarChar, 50).Value = Texto(vehiculo.Seguro);
            p.Add("@Modelo", SqlDbType.VarChar, 50).Value = Texto(vehiculo.Modelo);
            p.Add("@Marca", SqlDbType.VarChar, 50).Value = Texto(vehiculo.Marca);
            p.Add("@Nombre", SqlDbType.VarChar, 50).Value = Texto(vehiculo.Nombre);
            p.Add("@Caracteristica", SqlDbType.VarChar, 50).Value = Texto(vehiculo.Caracteristica);
        });

    public Task<string> CrearCelularAsync(NuevoCelularDto celular) => CrearAsync(
        TipoBien.Celular,
        @"
        INSERT INTO Celulares (Identificador, Marca, Modelo, Numero, Imei, Ram, Memoria, Caracteristica)
        VALUES (CONCAT('TMP-', @@SPID), @Marca, @Modelo, @Numero, @Imei, @Ram, @Memoria, @Caracteristica);

        DECLARE @Id INT = SCOPE_IDENTITY();
        DECLARE @Identificador VARCHAR(20) = CONCAT(@TipoBienId, '-', @Id);

        UPDATE Celulares SET Identificador = @Identificador WHERE Id = @Id;

        INSERT INTO Bienes (TipoBienId, BienId, Identificador, EstadoId)
        VALUES (@TipoBienId, @Id, @Identificador, @Disponible);

        SELECT @Identificador;",
        "Ya existe un celular con ese IMEI.",
        p =>
        {
            p.Add("@Marca", SqlDbType.VarChar, 50).Value = Texto(celular.Marca);
            p.Add("@Modelo", SqlDbType.VarChar, 50).Value = Texto(celular.Modelo);
            p.Add("@Numero", SqlDbType.VarChar, 50).Value = Texto(celular.Numero);
            p.Add("@Imei", SqlDbType.VarChar, 20).Value = Texto(celular.Imei);
            p.Add("@Ram", SqlDbType.VarChar, 50).Value = Texto(celular.Ram);
            p.Add("@Memoria", SqlDbType.VarChar, 50).Value = Texto(celular.Memoria);
            p.Add("@Caracteristica", SqlDbType.VarChar, 50).Value = Texto(celular.Caracteristica);
        });

    public Task<string> CrearLaptopAsync(NuevoLaptopDto laptop) => CrearAsync(
        TipoBien.Laptop,
        @"
        INSERT INTO Laptops (Identificador, Marca, Modelo, Serie, Ram, DiscoDuro, Procesador, Caracteristica)
        VALUES (CONCAT('TMP-', @@SPID), @Marca, @Modelo, @Serie, @Ram, @DiscoDuro, @Procesador, @Caracteristica);

        DECLARE @Id INT = SCOPE_IDENTITY();
        DECLARE @Identificador VARCHAR(20) = CONCAT(@TipoBienId, '-', @Id);

        UPDATE Laptops SET Identificador = @Identificador WHERE Id = @Id;

        INSERT INTO Bienes (TipoBienId, BienId, Identificador, EstadoId)
        VALUES (@TipoBienId, @Id, @Identificador, @Disponible);

        SELECT @Identificador;",
        "Ya existe una laptop con ese número de serie.",
        p =>
        {
            p.Add("@Marca", SqlDbType.VarChar, 50).Value = Texto(laptop.Marca);
            p.Add("@Modelo", SqlDbType.VarChar, 50).Value = Texto(laptop.Modelo);
            p.Add("@Serie", SqlDbType.VarChar, 50).Value = Texto(laptop.Serie);
            p.Add("@Ram", SqlDbType.VarChar, 50).Value = Texto(laptop.Ram);
            p.Add("@DiscoDuro", SqlDbType.VarChar, 50).Value = Texto(laptop.DiscoDuro);
            p.Add("@Procesador", SqlDbType.VarChar, 50).Value = Texto(laptop.Procesador);
            p.Add("@Caracteristica", SqlDbType.VarChar, 50).Value = Texto(laptop.Caracteristica);
        });

    public Task<string> CrearFondoFijoAsync(NuevoFondoFijoDto fondo) => CrearAsync(
        TipoBien.FondoFijo,
        @"
        INSERT INTO FondosFijos (Identificador, Monto)
        VALUES (CONCAT('TMP-', @@SPID), @Monto);

        DECLARE @Id INT = SCOPE_IDENTITY();
        DECLARE @Identificador VARCHAR(20) = CONCAT(@TipoBienId, '-', @Id);

        UPDATE FondosFijos SET Identificador = @Identificador WHERE Id = @Id;

        INSERT INTO Bienes (TipoBienId, BienId, Identificador, EstadoId)
        VALUES (@TipoBienId, @Id, @Identificador, @Disponible);

        SELECT @Identificador;",
        "No se pudo registrar el fondo fijo.",
        p =>
        {
            var monto = p.Add("@Monto", SqlDbType.Decimal);
            monto.Precision = 12;
            monto.Scale = 2;
            monto.Value = fondo.Monto;
        });

    public Task<string> CrearOtroAsync(NuevoOtroBienDto otro) => CrearAsync(
        TipoBien.Otros,
        @"
        INSERT INTO OtrosBienes (Identificador, Caracteristica1, Caracteristica2, Caracteristica3)
        VALUES (CONCAT('TMP-', @@SPID), @Caracteristica1, @Caracteristica2, @Caracteristica3);

        DECLARE @Id INT = SCOPE_IDENTITY();
        DECLARE @Identificador VARCHAR(20) = CONCAT(@TipoBienId, '-', @Id);

        UPDATE OtrosBienes SET Identificador = @Identificador WHERE Id = @Id;

        INSERT INTO Bienes (TipoBienId, BienId, Identificador, EstadoId)
        VALUES (@TipoBienId, @Id, @Identificador, @Disponible);

        SELECT @Identificador;",
        "No se pudo registrar el bien.",
        p =>
        {
            p.Add("@Caracteristica1", SqlDbType.VarChar, 50).Value = Texto(otro.Caracteristica1);
            p.Add("@Caracteristica2", SqlDbType.VarChar, 50).Value = Texto(otro.Caracteristica2);
            p.Add("@Caracteristica3", SqlDbType.VarChar, 50).Value = Texto(otro.Caracteristica3);
        });

    // ------------------------------------------------------------------
    // CAMBIOS DE ESTADO. Cada regla vive en el WHERE de su UPDATE.
    // ------------------------------------------------------------------
    public Task EnviarAMantenimientoAsync(string identificador) => CambiarEstadoAsync(
        @"
        UPDATE Bienes
        SET EstadoId = @Mantenimiento,
            UltimaModificacion = SYSDATETIME()
        WHERE Identificador = @Identificador
          AND EstadoId IN (@Disponible, @Asignado);",
        identificador,
        "Solo se puede enviar a mantenimiento un bien disponible o asignado.");

    // Al salir de mantenimiento, el bien vuelve a Asignado si su resguardo sigue activo
    // (el colaborador sigue siendo responsable), o a Disponible si no tiene.
    public Task TerminarMantenimientoAsync(string identificador) => CambiarEstadoAsync(
        @"
        UPDATE b
        SET b.EstadoId = CASE
                WHEN EXISTS (SELECT 1 FROM Resguardos r
                             WHERE r.IdentificadorBien = b.Identificador
                               AND r.EstadoId = @ResguardoActivo)
                THEN @Asignado
                ELSE @Disponible
            END,
            b.UltimaModificacion = SYSDATETIME()
        FROM Bienes b
        WHERE b.Identificador = @Identificador
          AND b.EstadoId = @Mantenimiento;",
        identificador,
        "El bien no está en mantenimiento.");

    public Task DarDeBajaAsync(string identificador) => CambiarEstadoAsync(
        @"
        UPDATE Bienes
        SET EstadoId = @Baja,
            UltimaModificacion = SYSDATETIME()
        WHERE Identificador = @Identificador
          AND EstadoId IN (@Disponible, @Mantenimiento)
          AND NOT EXISTS (SELECT 1 FROM Resguardos r
                          WHERE r.IdentificadorBien = @Identificador
                            AND r.EstadoId = @ResguardoActivo);",
        identificador,
        "No se puede dar de baja: el bien tiene un resguardo activo o ya está de baja.");

    // Ejecuta un alta completa en una transacción y devuelve el identificador generado.
    // Recibe SQL fijo escrito en esta misma clase; nunca texto que venga de afuera.
    private async Task<string> CrearAsync(
        TipoBien tipo,
        string sql,
        string mensajeDuplicado,
        Action<SqlParameterCollection> agregarParametros)
    {
        await using var conexion = new SqlConnection(_cadenaConexion);
        await conexion.OpenAsync();

        await using var transaccion = (SqlTransaction)await conexion.BeginTransactionAsync();

        try
        {
            await using var comando = new SqlCommand(sql, conexion, transaccion);
            comando.Parameters.Add("@TipoBienId", SqlDbType.Int).Value = (int)tipo;
            comando.Parameters.Add("@Disponible", SqlDbType.Int).Value = (int)EstadoBien.Disponible;
            agregarParametros(comando.Parameters);

            var identificador = (string)(await comando.ExecuteScalarAsync())!;

            await transaccion.CommitAsync();

            return identificador;
        }
        catch (SqlException ex) when (ex.Number is 2627 or 2601)
        {
            await transaccion.RollbackAsync();
            throw new InvalidOperationException(mensajeDuplicado);
        }
        catch
        {
            await transaccion.RollbackAsync();
            throw;
        }
    }

    private async Task CambiarEstadoAsync(string sql, string identificador, string mensajeSiNoAplica)
    {
        await using var conexion = new SqlConnection(_cadenaConexion);
        await using var comando = new SqlCommand(sql, conexion);

        comando.Parameters.Add("@Identificador", SqlDbType.VarChar, 20).Value = identificador;
        comando.Parameters.Add("@Disponible", SqlDbType.Int).Value = (int)EstadoBien.Disponible;
        comando.Parameters.Add("@Asignado", SqlDbType.Int).Value = (int)EstadoBien.Asignado;
        comando.Parameters.Add("@Mantenimiento", SqlDbType.Int).Value = (int)EstadoBien.Mantenimiento;
        comando.Parameters.Add("@Baja", SqlDbType.Int).Value = (int)EstadoBien.Baja;
        comando.Parameters.Add("@ResguardoActivo", SqlDbType.Int).Value = (int)EstadoResguardo.Activo;

        await conexion.OpenAsync();

        var filas = await comando.ExecuteNonQueryAsync();

        if (filas == 0)
            throw new InvalidOperationException(mensajeSiNoAplica);
    }

    // Texto vacío o solo espacios se guarda como NULL; lo demás, sin espacios en los extremos.
    private static object Texto(string? valor) =>
        string.IsNullOrWhiteSpace(valor) ? DBNull.Value : valor.Trim();
}
