/*
   VL221407Desafio2 - datos ficticios para demostración.
   Ejecutar después de Desafio2DB.sql.
   El script puede repetirse: usa los correos y títulos para evitar duplicados.
*/
USE Desafio2DB;
GO
SET NOCOUNT ON;
SET XACT_ABORT ON;
BEGIN TRANSACTION;

IF NOT EXISTS (SELECT 1 FROM dbo.Instructor WHERE Email = 'maria.lopez@example.com')
    INSERT dbo.Instructor (Nombre, Especialidad, Email)
    VALUES ('María López', 'Desarrollo de software', 'maria.lopez@example.com');

IF NOT EXISTS (SELECT 1 FROM dbo.Instructor WHERE Email = 'roberto.mendez@example.com')
    INSERT dbo.Instructor (Nombre, Especialidad, Email)
    VALUES ('Roberto Méndez', 'Bases de datos', 'roberto.mendez@example.com');

IF NOT EXISTS (SELECT 1 FROM dbo.Instructor WHERE Email = 'ana.martinez@example.com')
    INSERT dbo.Instructor (Nombre, Especialidad, Email)
    VALUES ('Ana Martínez', 'Diseño y desarrollo web', 'ana.martinez@example.com');

DECLARE @Maria int = (SELECT TOP (1) IdInstructor FROM dbo.Instructor WHERE Email = 'maria.lopez@example.com');
DECLARE @Roberto int = (SELECT TOP (1) IdInstructor FROM dbo.Instructor WHERE Email = 'roberto.mendez@example.com');
DECLARE @Ana int = (SELECT TOP (1) IdInstructor FROM dbo.Instructor WHERE Email = 'ana.martinez@example.com');

IF NOT EXISTS (SELECT 1 FROM dbo.Curso WHERE Titulo = 'Fundamentos de programación')
    INSERT dbo.Curso (Titulo, Descripcion, Nivel, IdInstructor) VALUES
    ('Fundamentos de programación', 'Aprende lógica, variables, condiciones y ciclos mediante ejercicios prácticos.', 'Básico', @Maria);
IF NOT EXISTS (SELECT 1 FROM dbo.Curso WHERE Titulo = 'APIs con ASP.NET Core')
    INSERT dbo.Curso (Titulo, Descripcion, Nivel, IdInstructor) VALUES
    ('APIs con ASP.NET Core', 'Construye APIs REST con arquitectura en capas, validaciones y documentación.', 'Avanzado', @Maria);
IF NOT EXISTS (SELECT 1 FROM dbo.Curso WHERE Titulo = 'Introducción a SQL Server')
    INSERT dbo.Curso (Titulo, Descripcion, Nivel, IdInstructor) VALUES
    ('Introducción a SQL Server', 'Diseña tablas, relaciones y consultas para organizar información.', 'Básico', @Roberto);
IF NOT EXISTS (SELECT 1 FROM dbo.Curso WHERE Titulo = 'Consultas y optimización SQL')
    INSERT dbo.Curso (Titulo, Descripcion, Nivel, IdInstructor) VALUES
    ('Consultas y optimización SQL', 'Practica joins, índices y transacciones para mejorar el acceso a datos.', 'Intermedio', @Roberto);
IF NOT EXISTS (SELECT 1 FROM dbo.Curso WHERE Titulo = 'Diseño web con HTML y CSS')
    INSERT dbo.Curso (Titulo, Descripcion, Nivel, IdInstructor) VALUES
    ('Diseño web con HTML y CSS', 'Crea interfaces accesibles y adaptables a computadoras y dispositivos móviles.', 'Básico', @Ana);
IF NOT EXISTS (SELECT 1 FROM dbo.Curso WHERE Titulo = 'JavaScript para aplicaciones web')
    INSERT dbo.Curso (Titulo, Descripcion, Nivel, IdInstructor) VALUES
    ('JavaScript para aplicaciones web', 'Integra formularios, eventos y consumo de APIs en aplicaciones interactivas.', 'Intermedio', @Ana);

IF NOT EXISTS (SELECT 1 FROM dbo.Estudiante WHERE Email = 'carlos.perez@example.com')
    INSERT dbo.Estudiante (Nombre, Email, FechaNacimiento) VALUES ('Carlos Pérez', 'carlos.perez@example.com', '20020515');
IF NOT EXISTS (SELECT 1 FROM dbo.Estudiante WHERE Email = 'sofia.hernandez@example.com')
    INSERT dbo.Estudiante (Nombre, Email, FechaNacimiento) VALUES ('Sofía Hernández', 'sofia.hernandez@example.com', '20030822');
IF NOT EXISTS (SELECT 1 FROM dbo.Estudiante WHERE Email = 'diego.ramirez@example.com')
    INSERT dbo.Estudiante (Nombre, Email, FechaNacimiento) VALUES ('Diego Ramírez', 'diego.ramirez@example.com', '20011110');
IF NOT EXISTS (SELECT 1 FROM dbo.Estudiante WHERE Email = 'valeria.gonzalez@example.com')
    INSERT dbo.Estudiante (Nombre, Email, FechaNacimiento) VALUES ('Valeria González', 'valeria.gonzalez@example.com', '20040218');
IF NOT EXISTS (SELECT 1 FROM dbo.Estudiante WHERE Email = 'andres.flores@example.com')
    INSERT dbo.Estudiante (Nombre, Email, FechaNacimiento) VALUES ('Andrés Flores', 'andres.flores@example.com', '20020903');
IF NOT EXISTS (SELECT 1 FROM dbo.Estudiante WHERE Email = 'camila.torres@example.com')
    INSERT dbo.Estudiante (Nombre, Email, FechaNacimiento) VALUES ('Camila Torres', 'camila.torres@example.com', '20030427');
IF NOT EXISTS (SELECT 1 FROM dbo.Estudiante WHERE Email = 'daniel.rivera@example.com')
    INSERT dbo.Estudiante (Nombre, Email, FechaNacimiento) VALUES ('Daniel Rivera', 'daniel.rivera@example.com', '20001206');
IF NOT EXISTS (SELECT 1 FROM dbo.Estudiante WHERE Email = 'lucia.castro@example.com')
    INSERT dbo.Estudiante (Nombre, Email, FechaNacimiento) VALUES ('Lucía Castro', 'lucia.castro@example.com', '20040714');

DECLARE @Inscripciones TABLE (Email varchar(100), Titulo varchar(150));
INSERT @Inscripciones (Email, Titulo) VALUES
('carlos.perez@example.com', 'Fundamentos de programación'),
('carlos.perez@example.com', 'Introducción a SQL Server'),
('sofia.hernandez@example.com', 'Diseño web con HTML y CSS'),
('sofia.hernandez@example.com', 'JavaScript para aplicaciones web'),
('diego.ramirez@example.com', 'APIs con ASP.NET Core'),
('diego.ramirez@example.com', 'Consultas y optimización SQL'),
('valeria.gonzalez@example.com', 'Fundamentos de programación'),
('valeria.gonzalez@example.com', 'Diseño web con HTML y CSS'),
('andres.flores@example.com', 'Introducción a SQL Server'),
('camila.torres@example.com', 'JavaScript para aplicaciones web'),
('daniel.rivera@example.com', 'APIs con ASP.NET Core'),
('lucia.castro@example.com', 'Diseño web con HTML y CSS');

INSERT dbo.Inscripcion (FechaInscripcion, IdEstudiante, IdCurso)
SELECT CONVERT(date, '20260910', 112), e.IdEstudiante, c.IdCurso
FROM @Inscripciones d
JOIN dbo.Estudiante e ON e.Email = d.Email
JOIN dbo.Curso c ON c.Titulo = d.Titulo
WHERE NOT EXISTS
(
    SELECT 1 FROM dbo.Inscripcion i
    WHERE i.IdEstudiante = e.IdEstudiante AND i.IdCurso = c.IdCurso
);

COMMIT TRANSACTION;

SELECT 'Instructor' Tabla, COUNT(*) Total FROM dbo.Instructor
UNION ALL SELECT 'Curso', COUNT(*) FROM dbo.Curso
UNION ALL SELECT 'Estudiante', COUNT(*) FROM dbo.Estudiante
UNION ALL SELECT 'Inscripcion', COUNT(*) FROM dbo.Inscripcion;
GO
