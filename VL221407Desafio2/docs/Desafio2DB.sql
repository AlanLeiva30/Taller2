/*
   VL221407Desafio2 - esquema portable para una base nueva.
   Docker demo ejecuta este archivo únicamente contra su servicio sqlserver.
   Las tablas existentes se conservan; no es un script de migración.
   No se incluyen rutas físicas del equipo, usuarios ni datos personales.
*/
USE master;
GO
IF DB_ID(N'Desafio2DB') IS NULL
    EXEC(N'CREATE DATABASE [Desafio2DB]');
GO
USE Desafio2DB;
GO
SET XACT_ABORT ON;
BEGIN TRANSACTION;

IF OBJECT_ID(N'dbo.Instructor', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Instructor
    (
        IdInstructor int IDENTITY(1,1) NOT NULL CONSTRAINT PK_Instructor PRIMARY KEY,
        Nombre varchar(100) NOT NULL,
        Especialidad varchar(100) NOT NULL,
        Email varchar(100) NOT NULL
    );
END;

IF OBJECT_ID(N'dbo.Curso', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Curso
    (
        IdCurso int IDENTITY(1,1) NOT NULL CONSTRAINT PK_Curso PRIMARY KEY,
        Titulo varchar(150) NOT NULL,
        Descripcion varchar(300) NOT NULL,
        Nivel varchar(50) NOT NULL,
        IdInstructor int NOT NULL,
        CONSTRAINT FK_Curso_Instructor FOREIGN KEY (IdInstructor)
            REFERENCES dbo.Instructor(IdInstructor),
        CONSTRAINT CK_Curso_Nivel CHECK
            (Nivel COLLATE Latin1_General_100_BIN2 IN ('Básico', 'Intermedio', 'Avanzado'))
    );
    CREATE INDEX IX_Curso_IdInstructor ON dbo.Curso(IdInstructor);
END;

IF OBJECT_ID(N'dbo.Estudiante', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Estudiante
    (
        IdEstudiante int IDENTITY(1,1) NOT NULL CONSTRAINT PK_Estudiante PRIMARY KEY,
        Nombre varchar(100) NOT NULL,
        Email varchar(100) NOT NULL,
        FechaNacimiento date NOT NULL
    );
END;

IF OBJECT_ID(N'dbo.Inscripcion', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Inscripcion
    (
        IdInscripcion int IDENTITY(1,1) NOT NULL CONSTRAINT PK_Inscripcion PRIMARY KEY,
        FechaInscripcion date NOT NULL,
        IdEstudiante int NOT NULL,
        IdCurso int NOT NULL,
        CONSTRAINT FK_Inscripcion_Estudiante FOREIGN KEY (IdEstudiante)
            REFERENCES dbo.Estudiante(IdEstudiante),
        CONSTRAINT FK_Inscripcion_Curso FOREIGN KEY (IdCurso)
            REFERENCES dbo.Curso(IdCurso),
        CONSTRAINT UQ_Inscripcion_Estudiante_Curso UNIQUE (IdEstudiante, IdCurso)
    );
    CREATE INDEX IX_Inscripcion_IdCurso ON dbo.Inscripcion(IdCurso);
END;

COMMIT TRANSACTION;
GO
