using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using VL221407Desafio2.Common;

namespace VL221407Desafio2.API.Infrastructure;

public sealed class ApiExceptionHandler(ILogger<ApiExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext context, Exception error, CancellationToken ct)
    {
        if (error is OperationCanceledException && context.RequestAborted.IsCancellationRequested)
        {
            context.Response.StatusCode = 499;
            return true;
        }
        var (status, detail) = error switch
        {
            AppException e => (e.StatusCode, e.Message),
            SqlException e when e.Number is 2601 or 2627 => (409, "Ya existe un registro con esos datos únicos."),
            SqlException e when e.Number == 547 => (409, "Hay registros relacionados. Verifica las referencias o elimina primero las inscripciones y cursos dependientes."),
            SqlException e when e.Number == 1205 => (409, "Otra operación modificó estos datos al mismo tiempo. Inténtalo de nuevo."),
            SqlException e when e.Number is 8152 or 2628 => (400, "Uno de los valores excede la longitud admitida por la base de datos."),
            SqlException e when e.Number is -2 or 2 or 53 or 64 or 233 or 4060 or 18456 or 10054 or 10060 or 11001 =>
                (503, "SQL Server no está disponible. Comprueba que el servicio esté iniciado y la conexión a Desafio2DB sea válida."),
            SqlException => (500, "No se pudo completar la operación de datos. Consulta el identificador de la solicitud en el registro de la API."),
            _ => (500, "Ocurrió un error inesperado. Inténtalo de nuevo.")
        };
        if (status >= 500) logger.LogError(error, "Error {Status} en {Path}; solicitud {TraceId}", status, context.Request.Path, context.TraceIdentifier);
        else logger.LogInformation("Solicitud rechazada: {Status} {Path} {Detail}", status, context.Request.Path, detail);
        var problem = new ProblemDetails
        {
            Status = status,
            Title = status switch { 400 => "Revisa los datos", 404 => "Registro no encontrado", 409 => "La operación presenta un conflicto", 503 => "Servicio no disponible", _ => "Error interno" },
            Detail = detail,
            Instance = context.Request.Path,
            Type = $"https://httpstatuses.com/{status}"
        };
        problem.Extensions["traceId"] = context.TraceIdentifier;
        context.Response.StatusCode = status;
        await context.Response.WriteAsJsonAsync(problem, options: (System.Text.Json.JsonSerializerOptions?)null, contentType: "application/problem+json", cancellationToken: ct);
        return true;
    }
}
