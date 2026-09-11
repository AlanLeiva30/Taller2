namespace VL221407Desafio2.Common;

public static class NivelesCurso
{
    public const string Basico = "Básico";
    public const string Intermedio = "Intermedio";
    public const string Avanzado = "Avanzado";

    public static bool EsValido(string nivel) => nivel is Basico or Intermedio or Avanzado;
}
