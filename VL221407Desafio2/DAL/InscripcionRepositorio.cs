using System.Data;
using Dapper;
using VL221407Desafio2.Common;
using VL221407Desafio2.Entities;

namespace VL221407Desafio2.DAL;

public class InscripcionRepositorio(ConexionSql conexion) : Repositorio<Inscripcion>(conexion), IInscripcionRepositorio
{
    public async Task<bool> ExisteAsync(int estudiante, int curso, int excluirId = 0, CancellationToken ct = default)
    {
        using var db = Conexion.Crear();
        return await db.ExecuteScalarAsync<int>(new CommandDefinition(
            "SELECT COUNT(*) FROM dbo.Inscripcion WHERE IdEstudiante=@estudiante AND IdCurso=@curso AND IdInscripcion<>@excluirId",
            new { estudiante, curso, excluirId }, cancellationToken: ct)) > 0;
    }

    // BL comprueba la regla antes de guardar. Esta transacción también protege solicitudes concurrentes.
    private async Task<int> GuardarAsync(int id, Inscripcion entidad, CancellationToken ct)
    {
        using var db = Conexion.Crear();
        await db.OpenAsync(ct);
        using var tx = db.BeginTransaction(IsolationLevel.Serializable);
        var repetidas = await db.ExecuteScalarAsync<int>(new CommandDefinition(
            "SELECT COUNT(*) FROM dbo.Inscripcion WITH (UPDLOCK,HOLDLOCK) WHERE IdEstudiante=@IdEstudiante AND IdCurso=@IdCurso AND IdInscripcion<>@id",
            new { entidad.IdEstudiante, entidad.IdCurso, id }, tx, cancellationToken: ct));
        if (repetidas > 0)
            throw new AppException("El estudiante ya está inscrito en este curso.", 409);

        var args = new { entidad.FechaInscripcion, entidad.IdEstudiante, entidad.IdCurso, id };
        int resultado = id == 0
            ? await db.QuerySingleAsync<int>(new CommandDefinition(
                "INSERT INTO dbo.Inscripcion (FechaInscripcion,IdEstudiante,IdCurso) OUTPUT INSERTED.IdInscripcion VALUES (@FechaInscripcion,@IdEstudiante,@IdCurso)",
                args, tx, cancellationToken: ct))
            : await db.ExecuteAsync(new CommandDefinition(
                "UPDATE dbo.Inscripcion SET FechaInscripcion=@FechaInscripcion,IdEstudiante=@IdEstudiante,IdCurso=@IdCurso WHERE IdInscripcion=@id",
                args, tx, cancellationToken: ct));
        await tx.CommitAsync(ct);
        return resultado;
    }

    public override Task<int> CrearAsync(Inscripcion entidad, CancellationToken ct = default) => GuardarAsync(0, entidad, ct);
    public override async Task<bool> ActualizarAsync(int id, Inscripcion entidad, CancellationToken ct = default)
        => await GuardarAsync(id, entidad, ct) > 0;
}
