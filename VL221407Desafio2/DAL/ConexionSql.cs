using Dapper;
using Microsoft.Data.SqlClient;

namespace VL221407Desafio2.DAL;

public class ConexionSql(string cadena)
{
    public SqlConnection Crear() => new(cadena);

    public async Task ComprobarAsync(CancellationToken ct = default)
    {
        using var db = Crear();
        await db.OpenAsync(ct);
        // Comprueba acceso al esquema requerido, sin leer datos personales.
        var tablas = await db.ExecuteScalarAsync<int>(new CommandDefinition("""
            SELECT COUNT(*) FROM sys.tables
            WHERE schema_id=SCHEMA_ID('dbo') AND name IN ('Instructor','Curso','Estudiante','Inscripcion')
            """, commandTimeout: 5, cancellationToken: ct));
        if (tablas != 4) throw new InvalidOperationException("Faltan tablas del esquema requerido.");
    }
}
