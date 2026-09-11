using System.ComponentModel.DataAnnotations;
namespace VL221407Desafio2.DTOs;
public class InstructorGuardarDto : IEntradaNormalizable
{
    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    [StringLength(100, ErrorMessage = "El campo {0} admite hasta {1} caracteres.")]
    public string Nombre { get; set; } = "";
    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    [StringLength(100, ErrorMessage = "El campo {0} admite hasta {1} caracteres.")]
    public string Especialidad { get; set; } = "";
    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    [StringLength(100, ErrorMessage = "El campo {0} admite hasta {1} caracteres.")]
    [EmailAddress(ErrorMessage = "Ingresa un correo electrónico válido.")]
    public string Email { get; set; } = "";
    public void Normalizar()
    {
        Nombre = Nombre?.Trim() ?? "";
        Especialidad = Especialidad?.Trim() ?? "";
        Email = Email?.Trim() ?? "";
    }
}

public class InstructorDto : InstructorGuardarDto
{
    public int IdInstructor { get; set; }

}
