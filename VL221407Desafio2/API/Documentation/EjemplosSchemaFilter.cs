using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using VL221407Desafio2.DTOs;

namespace VL221407Desafio2.API.Documentation;

public class EjemplosSchemaFilter : ISchemaFilter
{
    public void Apply(OpenApiSchema schema, SchemaFilterContext context)
    {
        foreach (var (name, property) in schema.Properties)
        {
            property.Example = name switch
            {
                "nombre" => new OpenApiString("María López"),
                "especialidad" => new OpenApiString("Desarrollo de software"),
                "email" => new OpenApiString("maria@example.com"),
                "titulo" => new OpenApiString("Fundamentos de programación"),
                "descripcion" => new OpenApiString("Lógica, algoritmos y estructuras de control."),
                "nivel" => new OpenApiString("Básico"),
                "nombreInstructor" => new OpenApiString("María López"),
                "fechaNacimiento" => new OpenApiString("2000-05-15"),
                "fechaInscripcion" => new OpenApiString(DateTime.Today.ToString("yyyy-MM-dd")),
                _ => property.Example
            };
            if (name == "nivel")
            {
                property.Enum = new List<IOpenApiAny> { new OpenApiString("Básico"), new OpenApiString("Intermedio"), new OpenApiString("Avanzado") };
                property.Description = "Uno de los tres niveles permitidos.";
            }
            if (name.StartsWith("id", StringComparison.Ordinal))
            {
                property.Example = new OpenApiInteger(1);
                property.Description = "Identificador del registro. Usa un ID existente en tu base.";
                property.Minimum = 1;
            }
            if (name.StartsWith("fecha", StringComparison.Ordinal)) property.Format = "date";
        }
    }
}
