using Dapper;
using VL221407Desafio2.Entities;

namespace VL221407Desafio2.DAL;

// Tabla y columnas se obtienen de una lista interna; todos los valores son parámetros SQL.
public class Repositorio<T>(ConexionSql conexion) : IRepositorio<T> where T : class
{
    protected ConexionSql Conexion { get; } = conexion;
    private static readonly Dictionary<Type, (string Tabla, string Campos)> Mapas = new()
    {
        [typeof(Instructor)] = ("Instructor", "Nombre,Especialidad,Email"),
        [typeof(Curso)] = ("Curso", "Titulo,Descripcion,Nivel,IdInstructor"),
        [typeof(Estudiante)] = ("Estudiante", "Nombre,Email,FechaNacimiento"),
        [typeof(Inscripcion)] = ("Inscripcion", "FechaInscripcion,IdEstudiante,IdCurso")
    };
    private static readonly (string Tabla, string Campos) Mapa = Mapas[typeof(T)];
    private static string Seleccion => typeof(T) == typeof(Curso)
        ? "SELECT t.IdCurso,t.Titulo,t.Descripcion,t.Nivel,t.IdInstructor,i.Nombre AS NombreInstructor FROM dbo.Curso t INNER JOIN dbo.Instructor i ON i.IdInstructor=t.IdInstructor"
        : $"SELECT t.Id{Mapa.Tabla},{string.Join(",", Mapa.Campos.Split(',').Select(c => "t." + c))} FROM dbo.{Mapa.Tabla} t";

    public async Task<IEnumerable<T>> ListarAsync(CancellationToken ct = default)
    {
        using var db = Conexion.Crear();
        return await db.QueryAsync<T>(new CommandDefinition($"{Seleccion} ORDER BY t.Id{Mapa.Tabla}", cancellationToken: ct));
    }

    public async Task<T?> ObtenerAsync(int id, CancellationToken ct = default)
    {
        using var db = Conexion.Crear();
        return await db.QuerySingleOrDefaultAsync<T>(new CommandDefinition($"{Seleccion} WHERE t.Id{Mapa.Tabla}=@id", new { id }, cancellationToken: ct));
    }

    public virtual async Task<int> CrearAsync(T entidad, CancellationToken ct = default)
    {
        using var db = Conexion.Crear();
        var valores = string.Join(",", Mapa.Campos.Split(',').Select(c => "@" + c));
        return await db.QuerySingleAsync<int>(new CommandDefinition(
            $"INSERT INTO dbo.{Mapa.Tabla} ({Mapa.Campos}) OUTPUT INSERTED.Id{Mapa.Tabla} VALUES ({valores})",
            entidad, cancellationToken: ct));
    }

    public virtual async Task<bool> ActualizarAsync(int id, T entidad, CancellationToken ct = default)
    {
        using var db = Conexion.Crear();
        var cambios = string.Join(",", Mapa.Campos.Split(',').Select(c => $"{c}=@{c}"));
        var parametros = new DynamicParameters(entidad);
        parametros.Add("IdRegistro", id);
        return await db.ExecuteAsync(new CommandDefinition(
            $"UPDATE dbo.{Mapa.Tabla} SET {cambios} WHERE Id{Mapa.Tabla}=@IdRegistro",
            parametros, cancellationToken: ct)) > 0;
    }

    public async Task<bool> EliminarAsync(int id, CancellationToken ct = default)
    {
        using var db = Conexion.Crear();
        return await db.ExecuteAsync(new CommandDefinition(
            $"DELETE FROM dbo.{Mapa.Tabla} WHERE Id{Mapa.Tabla}=@id", new { id }, cancellationToken: ct)) > 0;
    }
}
