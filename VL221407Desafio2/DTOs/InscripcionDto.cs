using System.ComponentModel.DataAnnotations;
namespace VL221407Desafio2.DTOs;
public class InscripcionGuardarDto : IEntradaNormalizable
{
    [Required]
    public DateTime FechaInscripcion { get; set; }
    [Range(1, int.MaxValue, ErrorMessage = "Selecciona un registro válido para {0}.")]
    public int IdEstudiante { get; set; }
    [Range(1, int.MaxValue, ErrorMessage = "Selecciona un registro válido para {0}.")]
    public int IdCurso { get; set; }
    public void Normalizar()
    {
        FechaInscripcion = FechaInscripcion.Date;
    }
}

public class InscripcionDto : InscripcionGuardarDto
{
    public int IdInscripcion { get; set; }

}
