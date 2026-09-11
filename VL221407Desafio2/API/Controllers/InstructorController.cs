using Microsoft.AspNetCore.Mvc;
using VL221407Desafio2.BL;
using VL221407Desafio2.DTOs;

namespace VL221407Desafio2.API.Controllers;

/// <summary>Administración de instructores.</summary>
[ApiController]
[Produces("application/json")]
[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status503ServiceUnavailable)]
[Route("api/instructor")]
public class InstructorController(InstructorServicio servicio) : ControllerBase
{
    /// <summary>Consultar todos los instructores.</summary>
    /// <remarks>Devuelve una lista vacía cuando no existen registros.</remarks>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<InstructorDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<InstructorDto>>> Listar(CancellationToken ct)
        => Ok(await servicio.ListarAsync(ct));

    /// <summary>Consultar un registro por su identificador.</summary>
    /// <param name="id">Identificador entero mayor que cero.</param>
    /// <param name="ct">Cancelación de la solicitud.</param>
    [HttpGet("{id:int:min(1)}")]
    [ProducesResponseType(typeof(InstructorDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<InstructorDto>> Obtener(int id, CancellationToken ct)
        => Ok(await servicio.ObtenerAsync(id, ct));

    /// <summary>Crear un registro.</summary>
    /// <remarks>Los campos son obligatorios; se validan longitudes y formato del correo.</remarks>
    [HttpPost]
    [ProducesResponseType(typeof(InstructorDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<InstructorDto>> Crear(InstructorGuardarDto dto, CancellationToken ct)
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
    public async Task<IActionResult> Actualizar(int id, InstructorGuardarDto dto, CancellationToken ct)
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
