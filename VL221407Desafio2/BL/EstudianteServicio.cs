using AutoMapper;
using VL221407Desafio2.DTOs;
using VL221407Desafio2.Common;
using VL221407Desafio2.DAL;
using VL221407Desafio2.Entities;

namespace VL221407Desafio2.BL;

public class EstudianteServicio(IRepositorio<Estudiante> repo, IMapper mapper, CalendarioAcademico calendario)
    : ServicioCrud<Estudiante, EstudianteDto, EstudianteGuardarDto>(repo, mapper)
{
    protected override Task ValidarAsync(EstudianteGuardarDto dto, int id, CancellationToken ct)
    {
        if (dto.FechaNacimiento == default || dto.FechaNacimiento.Date > calendario.Hoy)
            throw new AppException("La fecha de nacimiento es obligatoria y no puede ser futura.");
        return Task.CompletedTask;
    }
}
