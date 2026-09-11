using AutoMapper;
using VL221407Desafio2.Entities;
using VL221407Desafio2.DTOs;
namespace VL221407Desafio2.BL;
public class MapeoPerfil : Profile
{
    public MapeoPerfil()
    {
        CreateMap<Instructor, InstructorDto>().ReverseMap();
        CreateMap<InstructorGuardarDto, Instructor>().ForMember(d => d.IdInstructor, o => o.Ignore());
        CreateMap<Curso, CursoDto>().ReverseMap();
        CreateMap<CursoGuardarDto, Curso>().ForMember(d => d.IdCurso, o => o.Ignore()).ForMember(d => d.NombreInstructor, o => o.Ignore());
        CreateMap<Estudiante, EstudianteDto>().ReverseMap();
        CreateMap<EstudianteGuardarDto, Estudiante>().ForMember(d => d.IdEstudiante, o => o.Ignore());
        CreateMap<Inscripcion, InscripcionDto>().ReverseMap();
        CreateMap<InscripcionGuardarDto, Inscripcion>().ForMember(d => d.IdInscripcion, o => o.Ignore());
    }
}
