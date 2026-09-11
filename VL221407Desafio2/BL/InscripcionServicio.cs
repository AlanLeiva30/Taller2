using AutoMapper;
using VL221407Desafio2.DTOs;
using VL221407Desafio2.Common;
using VL221407Desafio2.DAL;
using VL221407Desafio2.Entities;

namespace VL221407Desafio2.BL;

public class InscripcionServicio(IInscripcionRepositorio repo, IRepositorio<Estudiante> estudiantes, IRepositorio<Curso> cursos, IMapper mapper)
    : ServicioCrud<Inscripcion, InscripcionDto, InscripcionGuardarDto>(repo, mapper)
{
    protected override async Task ValidarAsync(InscripcionGuardarDto dto, int id, CancellationToken ct)
    {
        if (dto.FechaInscripcion == default)
            throw new AppException("La fecha de inscripción es obligatoria.");
        if (await estudiantes.ObtenerAsync(dto.IdEstudiante, ct) is null)
            throw new AppException("El estudiante indicado no existe.");
        if (await cursos.ObtenerAsync(dto.IdCurso, ct) is null)
            throw new AppException("El curso indicado no existe.");
        if (await repo.ExisteAsync(dto.IdEstudiante, dto.IdCurso, id, ct))
            throw new AppException("El estudiante ya está inscrito en este curso.", 409);
    }
}
