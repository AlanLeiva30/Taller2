using AutoMapper;
using VL221407Desafio2.DTOs;
using VL221407Desafio2.DAL;
using VL221407Desafio2.Entities;

namespace VL221407Desafio2.BL;

public class InstructorServicio(IRepositorio<Instructor> repo, IMapper mapper)
    : ServicioCrud<Instructor, InstructorDto, InstructorGuardarDto>(repo, mapper);
