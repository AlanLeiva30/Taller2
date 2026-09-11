using System.ComponentModel.DataAnnotations;
namespace VL221407Desafio2.DTOs;
public class CursoGuardarDto : IEntradaNormalizable
{
    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    [StringLength(150, ErrorMessage = "El campo {0} admite hasta {1} caracteres.")]
    public string Titulo { get; set; } = "";
    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    [StringLength(300, ErrorMessage = "El campo {0} admite hasta {1} caracteres.")]
    public string Descripcion { get; set; } = "";
    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    [StringLength(50, ErrorMessage = "El campo {0} admite hasta {1} caracteres.")]
    public string Nivel { get; set; } = "";
    [Range(1, int.MaxValue, ErrorMessage = "Selecciona un registro válido para {0}.")]
    public int IdInstructor { get; set; }
    public void Normalizar()
    {
        Titulo = Titulo?.Trim() ?? "";
        Descripcion = Descripcion?.Trim() ?? "";
        Nivel = Nivel?.Trim() ?? "";
    }
}

public class CursoDto : CursoGuardarDto
{
    public int IdCurso { get; set; }
    public string NombreInstructor { get; set; } = "";
}
