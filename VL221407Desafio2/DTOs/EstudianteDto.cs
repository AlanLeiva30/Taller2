using System.ComponentModel.DataAnnotations;
namespace VL221407Desafio2.DTOs;
public class EstudianteGuardarDto : IEntradaNormalizable
{
    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    [StringLength(100, ErrorMessage = "El campo {0} admite hasta {1} caracteres.")]
    public string Nombre { get; set; } = "";
    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    [StringLength(100, ErrorMessage = "El campo {0} admite hasta {1} caracteres.")]
    [EmailAddress(ErrorMessage = "Ingresa un correo electrónico válido.")]
    public string Email { get; set; } = "";
    [Required]
    public DateTime FechaNacimiento { get; set; }
    public void Normalizar()
    {
        Nombre = Nombre?.Trim() ?? "";
        Email = Email?.Trim() ?? "";
        FechaNacimiento = FechaNacimiento.Date;
    }
}

public class EstudianteDto : EstudianteGuardarDto
{
    public int IdEstudiante { get; set; }

}
