using AutoMapper;
using VL221407Desafio2.DTOs;
using VL221407Desafio2.Common;
using VL221407Desafio2.DAL;
using VL221407Desafio2.Entities;

namespace VL221407Desafio2.BL;

public class CursoServicio(IRepositorio<Curso> repo, IRepositorio<Instructor> instructores, IMapper mapper)
    : ServicioCrud<Curso, CursoDto, CursoGuardarDto>(repo, mapper)
{
    protected override async Task ValidarAsync(CursoGuardarDto dto, int id, CancellationToken ct)
    {
        if (!NivelesCurso.EsValido(dto.Nivel))
            throw new AppException("El nivel debe ser Básico, Intermedio o Avanzado.");
        if (await instructores.ObtenerAsync(dto.IdInstructor, ct) is null)
            throw new AppException("El instructor indicado no existe.");
    }
}
