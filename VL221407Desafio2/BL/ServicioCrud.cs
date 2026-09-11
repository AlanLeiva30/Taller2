using AutoMapper;
using System.ComponentModel.DataAnnotations;
using VL221407Desafio2.Common;
using VL221407Desafio2.DAL;
using VL221407Desafio2.DTOs;

namespace VL221407Desafio2.BL;

public class ServicioCrud<T, TDto, TGuardar>(IRepositorio<T> repositorio, IMapper mapper)
    where T : class where TGuardar : class, IEntradaNormalizable
{
    protected virtual Task ValidarAsync(TGuardar dto, int id, CancellationToken ct) => Task.CompletedTask;

    private async Task ValidarEntradaAsync(TGuardar dto, int id, CancellationToken ct)
    {
        dto.Normalizar();
        var errores = new List<ValidationResult>();
        if (!Validator.TryValidateObject(dto, new ValidationContext(dto), errores, true))
            throw new AppException(string.Join(" ", errores.Select(e => e.ErrorMessage)));
        await ValidarAsync(dto, id, ct);
    }

    public async Task<IEnumerable<TDto>> ListarAsync(CancellationToken ct = default)
        => mapper.Map<IEnumerable<TDto>>(await repositorio.ListarAsync(ct));

    public async Task<TDto> ObtenerAsync(int id, CancellationToken ct = default)
    {
        if (id < 1) throw new AppException("El identificador debe ser mayor que cero.");
        var entidad = await repositorio.ObtenerAsync(id, ct) ?? throw new AppException("Registro no encontrado.", 404);
        return mapper.Map<TDto>(entidad);
    }

    public async Task<int> CrearAsync(TGuardar dto, CancellationToken ct = default)
    {
        await ValidarEntradaAsync(dto, 0, ct);
        return await repositorio.CrearAsync(mapper.Map<T>(dto), ct);
    }

    public async Task ActualizarAsync(int id, TGuardar dto, CancellationToken ct = default)
    {
        _ = await ObtenerAsync(id, ct);
        await ValidarEntradaAsync(dto, id, ct);
        if (!await repositorio.ActualizarAsync(id, mapper.Map<T>(dto), ct))
            throw new AppException("Registro no encontrado.", 404);
    }

    public async Task EliminarAsync(int id, CancellationToken ct = default)
    {
        if (id < 1) throw new AppException("El identificador debe ser mayor que cero.");
        if (!await repositorio.EliminarAsync(id, ct))
            throw new AppException("Registro no encontrado.", 404);
    }
}
