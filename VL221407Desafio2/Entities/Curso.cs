namespace VL221407Desafio2.Entities;
public class Curso
{
    public int IdCurso { get; set; }
    public string Titulo { get; set; } = "";
    public string Descripcion { get; set; } = "";
    public string Nivel { get; set; } = "";
    public int IdInstructor { get; set; }
    public string NombreInstructor { get; set; } = "";
}

