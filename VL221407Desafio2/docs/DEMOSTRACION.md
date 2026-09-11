# Guía de demostración · VL221407Desafio2

Recorrido sugerido para un video de 5–8 minutos. Ejecuta primero la API y comprueba `/health/ready`. Usa registros creados para la demostración y guarda los identificadores devueltos: no asumas que comienzan en `1`.

## 1. Explicar las capas

Muestra la solución en Visual Studio: API recibe HTTP; BL valida reglas; DAL usa Dapper; Entities representa las tablas; DTOs contiene los contratos; Common contiene excepciones compartidas. Enseña un controlador, un servicio, un repositorio y el perfil de AutoMapper para seguir una solicitud de principio a fin.

## 2. Crear instructor y estudiante

En Swagger, selecciona **Try it out** y ejecuta `POST /api/instructor`:

```json
{
  "nombre": "Instructor de demostración",
  "especialidad": "Desarrollo de software",
  "email": "instructor.demo@example.com"
}
```

Guarda `idInstructor`. Ejecuta `POST /api/estudiante`:

```json
{
  "nombre": "Estudiante de demostración",
  "email": "estudiante.demo@example.com",
  "fechaNacimiento": "2003-05-15"
}
```

Guarda `idEstudiante`. Ambas creaciones deben responder `201`.

## 3. Crear y consultar un curso

Ejecuta `POST /api/curso` reemplazando `idInstructor` por el que recibiste:

```json
{
  "titulo": "API REST con ASP.NET Core",
  "descripcion": "Curso de demostración sobre arquitectura N-Capas y Dapper.",
  "nivel": "Básico",
  "idInstructor": 1
}
```

Guarda `idCurso`. Ejecuta `GET /api/curso` y muestra el campo `nombreInstructor`. Prueba además un nivel como `Experto`: debe devolver `400`. Un instructor inexistente también debe rechazarse.

## 4. Inscripción y regla de duplicados

Ejecuta `POST /api/inscripcion` con los identificadores reales y una fecha válida:

```json
{
  "fechaInscripcion": "2026-09-10",
  "idEstudiante": 1,
  "idCurso": 1
}
```

Guarda `idInscripcion`. Repite la misma solicitud: debe devolver `409`. Explica que BL comprueba la regla y DAL protege también escrituras simultáneas; la base nueva incluye una restricción única sobre estudiante/curso.

## 5. Editar y eliminar

Actualiza el curso con `PUT /api/curso/{idCurso}`, enviando todos los campos del cuerpo y cambiando el nivel a `Intermedio`. Debe responder `204`. Consulta de nuevo para mostrar el cambio.

Elimina sólo los registros de demostración en este orden:

1. `DELETE /api/inscripcion/{idInscripcion}`.
2. `DELETE /api/curso/{idCurso}`.
3. `DELETE /api/estudiante/{idEstudiante}`.
4. `DELETE /api/instructor/{idInstructor}`.

Cada eliminación debe responder `204`; consulta un ID eliminado para mostrar el `404`. Si intentas borrar un instructor que todavía tiene cursos, se espera `409`.

## 6. Mostrar Docker funcionando

Si usas la demo aislada del README, muestra `docker compose -f compose.demo.yaml ps`, la API saludable y el portal/Swagger en el puerto `5081`. Explica que su SQL Server tiene un volumen propio y que Visual Studio usa tu base existente con Windows Authentication.

La existencia de un Dockerfile por sí sola no demuestra que la tecnología funcione: muestra el contenedor ejecutándose y una consulta correcta.

## Entrega

Incluye la solución, el script SQL y los archivos Docker en el enlace del proyecto. El PDF debe contener los enlaces accesibles del proyecto y del video. Revisa en el aula el horario y formato de entrega indicados por la docente.
