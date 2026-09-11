using Microsoft.AspNetCore.Mvc;
using VL221407Desafio2.BL;
using VL221407Desafio2.DTOs;

namespace VL221407Desafio2.API.Controllers;

/// <summary>Administración de inscripciones.</summary>
[ApiController]
[Produces("application/json")]
[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status503ServiceUnavailable)]
[Route("api/inscripcion")]
public class InscripcionController(InscripcionServicio servicio) : ControllerBase
{
    /// <summary>Consultar todas las inscripciones.</summary>
    /// <remarks>Devuelve una lista vacía cuando no existen registros.</remarks>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<InscripcionDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<InscripcionDto>>> Listar(CancellationToken ct)
        => Ok(await servicio.ListarAsync(ct));

    /// <summary>Consultar un registro por su identificador.</summary>
    /// <param name="id">Identificador entero mayor que cero.</param>
    /// <param name="ct">Cancelación de la solicitud.</param>
    [HttpGet("{id:int:min(1)}")]
    [ProducesResponseType(typeof(InscripcionDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<InscripcionDto>> Obtener(int id, CancellationToken ct)
        => Ok(await servicio.ObtenerAsync(id, ct));

    /// <summary>Crear un registro.</summary>
    /// <remarks>El estudiante y el curso deben existir. Una inscripción duplicada devuelve 409.</remarks>
    [HttpPost]
    [ProducesResponseType(typeof(InscripcionDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<InscripcionDto>> Crear(InscripcionGuardarDto dto, CancellationToken ct)
    {
        var id = await servicio.CrearAsync(dto, ct);
        return CreatedAtAction(nameof(Obtener), new { id }, await servicio.ObtenerAsync(id, ct));
    }

    /// <summary>Actualizar todos los campos de un registro.</summary>
    /// <remarks>Requiere enviar todos los campos del cuerpo. Un resultado 204 confirma la actualización.</remarks>
    [HttpPut("{id:int:min(1)}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Actualizar(int id, InscripcionGuardarDto dto, CancellationToken ct)
    {
        await servicio.ActualizarAsync(id, dto, ct);
        return NoContent();
    }

    /// <summary>Eliminar un registro.</summary>
    /// <remarks>Devuelve 409 si existen registros relacionados. Elimina primero las dependencias.</remarks>
    [HttpDelete("{id:int:min(1)}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Eliminar(int id, CancellationToken ct)
    {
        await servicio.EliminarAsync(id, ct);
        return NoContent();
    }
}
