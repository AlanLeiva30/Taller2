namespace VL221407Desafio2.Common;

/// <summary>Fecha de negocio independiente de la zona horaria del servidor o contenedor.</summary>
public sealed class CalendarioAcademico(TimeProvider reloj, string zonaHoraria)
{
    private readonly TimeZoneInfo zona = TimeZoneInfo.FindSystemTimeZoneById(zonaHoraria);
    public DateTime Hoy => TimeZoneInfo.ConvertTime(reloj.GetUtcNow(), zona).Date;
}
