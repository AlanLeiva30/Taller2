#requires -Version 7.0
<#
.SYNOPSIS
Comprueba Swagger y los endpoints; opcionalmente ejecuta el CRUD con datos temporales.
.EXAMPLE
pwsh -File ./scripts/Test-Api.ps1
.EXAMPLE
pwsh -File ./scripts/Test-Api.ps1 -BaseUrl http://localhost:5081 -AllowWrites
.DESCRIPTION
Sin -AllowWrites solo se realizan consultas. La integración crea registros con una
marca única, comprueba el CRUD y las reglas, y elimina únicamente los IDs creados
por esta ejecución, incluso si falla una comprobación. No modifica el esquema.
Los valores IDENTITY consumidos durante las pruebas no se restablecen.
La API y SQL Server deben estar en ejecución. Un fallo devuelve exit code 1.
#>
[CmdletBinding()]
param(
    [ValidatePattern('^https?://')]
    [string] $BaseUrl = 'http://localhost:5080',
    [switch] $AllowWrites,
    [ValidateRange(1, 120)]
    [int] $TimeoutSeconds = 30
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
$BaseUrl = $BaseUrl.TrimEnd('/')
$script:checks = [System.Collections.Generic.List[object]]::new()
$script:created = @{
    instructor = [System.Collections.Generic.List[int]]::new()
    curso = [System.Collections.Generic.List[int]]::new()
    estudiante = [System.Collections.Generic.List[int]]::new()
    inscripcion = [System.Collections.Generic.List[int]]::new()
}
$idFields = @{
    instructor = 'IdInstructor'
    curso = 'IdCurso'
    estudiante = 'IdEstudiante'
    inscripcion = 'IdInscripcion'
}
$started = Get-Date
$script:activeCheck = 'Conectar a la API'

function Get-Field {
    param($Object, [string] $Name)
    if ($null -eq $Object) { return $null }
    $property = $Object.PSObject.Properties | Where-Object Name -IEQ $Name | Select-Object -First 1
    if ($null -ne $property) { return $property.Value }
    return $null
}

function Add-Check {
    param([string] $Name, [bool] $Passed, [string] $Detail = '')
    $script:checks.Add([pscustomobject]@{ Prueba = $Name; Correcta = $Passed; Detalle = $Detail })
    if ($Passed) { Write-Host "[OK] $Name" -ForegroundColor Green }
    else { Write-Host "[ERROR] $Name - $Detail" -ForegroundColor Red }
}

function Assert-True {
    param([bool] $Condition, [string] $Name, [string] $Detail = 'El resultado no coincide con lo esperado.')
    $script:activeCheck = $Name
    if (-not $Condition) { throw $Detail }
    Add-Check -Name $Name -Passed $true
}

function Invoke-Api {
    param([string] $Method, [string] $Path, $Body = $null)
    $parameters = @{
        Uri = "$BaseUrl$Path"
        Method = $Method
        TimeoutSec = $TimeoutSeconds
        SkipHttpErrorCheck = $true
        Headers = @{ Accept = 'application/json' }
    }
    if ($null -ne $Body) {
        # Bytes UTF-8 evitan alterar Básico y los nombres acentuados.
        $parameters.ContentType = 'application/json; charset=utf-8'
        $parameters.Body = [System.Text.Encoding]::UTF8.GetBytes(($Body | ConvertTo-Json -Depth 12 -Compress))
    }
    $response = Invoke-WebRequest @parameters
    $json = $null
    $responseText = if ($response.Content -is [byte[]]) { [System.Text.Encoding]::UTF8.GetString($response.Content) } else { [string]$response.Content }
    if (-not [string]::IsNullOrWhiteSpace($responseText)) {
        try { $json = $responseText | ConvertFrom-Json -Depth 30 }
        catch { $json = $null }
    }

    # Registrar antes de evaluar el estado esperado: también se limpian los POST
    # que una implementación defectuosa hubiera aceptado indebidamente.
    if ($Method -eq 'POST' -and [int]$response.StatusCode -eq 201 -and $Path -match '^/api/(instructor|curso|estudiante|inscripcion)$') {
        $entity = $Matches[1]
        $newId = 0
        $bodyId = Get-Field -Object $json -Name $idFields[$entity]
        $foundId = [int]::TryParse([string]$bodyId, [ref]$newId) -and $newId -gt 0
        if (-not $foundId) {
            $location = [string](@($response.Headers.Location) -join '')
            if ($location -match ('/api/' + $entity + '/([1-9][0-9]*)(?:\?.*)?$')) {
                $foundId = [int]::TryParse($Matches[1], [ref]$newId) -and $newId -gt 0
            }
        }
        if ($foundId -and -not $script:created[$entity].Contains($newId)) {
            $script:created[$entity].Add($newId)
        }
        elseif (-not $foundId) {
            throw "POST $Path devolvió 201 sin ID ni Location utilizable. No es posible identificar ese registro para limpiarlo."
        }
    }
    return [pscustomobject]@{ Status = [int]$response.StatusCode; Json = $json; Headers = $response.Headers }
}

function Expect-Request {
    param([string] $Method, [string] $Path, [int] $Status, [string] $Name, $Body = $null)
    $script:activeCheck = $Name
    $result = Invoke-Api -Method $Method -Path $Path -Body $Body
    if ($result.Status -ne $Status) {
        $detail = Get-Field -Object $result.Json -Name 'detail'
        throw "${Method} ${Path}: se esperaba HTTP $Status, se recibió $($result.Status). $detail"
    }
    Add-Check -Name $Name -Passed $true
    return $result
}

function New-TestRecord {
    param([string] $Entity, $Body, [string] $Name)
    $result = Expect-Request -Method POST -Path "/api/$Entity" -Status 201 -Name $Name -Body $Body
    $id = [int](Get-Field -Object $result.Json -Name $idFields[$Entity])
    Assert-True -Condition ($id -gt 0) -Name "$Entity devuelve su ID"
    Assert-True -Condition ($null -ne $result.Headers.Location) -Name "$Entity devuelve Location"
    return $id
}

function Find-MissingId {
    param([string] $Entity)
    # Confirmar ausencia primero evita asumir que un ID alto no existe.
    for ($offset = 0; $offset -lt 16; $offset++) {
        $candidate = [int]::MaxValue - $offset
        $script:activeCheck = "GET $Entity inexistente"
        $result = Invoke-Api -Method GET -Path "/api/$Entity/$candidate"
        if ($result.Status -eq 404) {
            Add-Check -Name $script:activeCheck -Passed $true
            return $candidate
        }
        if ($result.Status -ne 200) { throw "No se pudo comprobar un ID inexistente de $Entity (HTTP $($result.Status))." }
    }
    throw "No se encontró un ID libre de $Entity para comprobar las validaciones."
}

function Assert-SavedFields {
    param([string] $Entity, [int] $Id, [hashtable] $Expected, [string] $Name)
    $result = Expect-Request -Method GET -Path "/api/$Entity/$Id" -Status 200 -Name "$Name - GET"
    foreach ($field in $Expected.Keys) {
        $actual = Get-Field -Object $result.Json -Name $field
        $wanted = $Expected[$field]
        if ($field -like 'Fecha*') {
            $actual = if ($null -eq $actual) { '' } else { ([datetime]$actual).ToString('yyyy-MM-dd') }
            $wanted = ([datetime]$wanted).ToString('yyyy-MM-dd')
        }
        Assert-True -Condition ([string]$actual -ceq [string]$wanted) -Name "$Name - $field"
    }
}

function Remove-TestRecord {
    param([string] $Entity, [int] $Id, [switch] $Cleanup)
    if (-not $script:created[$Entity].Contains($Id)) { throw "Se rechazó borrar $Entity/${Id}: no fue creado por esta ejecución." }
    $script:activeCheck = "DELETE $Entity/$Id"
    $result = Invoke-Api -Method DELETE -Path "/api/$Entity/$Id"
    if ($result.Status -eq 204 -or ($Cleanup -and $result.Status -eq 404)) {
        [void]$script:created[$Entity].Remove($Id)
        Add-Check -Name "$script:activeCheck$(if ($Cleanup) { ' (limpieza)' })" -Passed $true
        if (-not $Cleanup) {
            $null = Expect-Request -Method GET -Path "/api/$Entity/$Id" -Status 404 -Name "$Entity eliminado devuelve 404"
        }
    }
    else { throw "La eliminación de $Entity/$Id devolvió HTTP $($result.Status)." }
}

try {
    Write-Host "API: $BaseUrl | Modo: $(if ($AllowWrites) { 'integración con registros temporales' } else { 'solo lectura' })"
    $null = Expect-Request -Method GET -Path '/health' -Status 200 -Name 'API disponible'
    $swagger = Expect-Request -Method GET -Path '/swagger/v1/swagger.json' -Status 200 -Name 'OpenAPI disponible'
    $paths = Get-Field -Object $swagger.Json -Name 'paths'
    foreach ($entity in @('instructor', 'curso', 'estudiante', 'inscripcion')) {
        $collection = Get-Field -Object $paths -Name "/api/$entity"
        $item = Get-Field -Object $paths -Name "/api/$entity/{id}"
        foreach ($method in @('get', 'post')) {
            Assert-True -Condition ($null -ne (Get-Field -Object $collection -Name $method)) -Name "OpenAPI documenta $method /api/$entity"
        }
        foreach ($method in @('get', 'put', 'delete')) {
            Assert-True -Condition ($null -ne (Get-Field -Object $item -Name $method)) -Name "OpenAPI documenta $method /api/$entity/{id}"
        }
        $null = Expect-Request -Method GET -Path "/api/$entity" -Status 200 -Name "Consultar listado de $entity"
    }

    if ($AllowWrites) {
        $tag = 'QA-' + [guid]::NewGuid().ToString('N').Substring(0, 12)
        Write-Host "Marca de esta ejecución: $tag"
        $missing = @{}
        foreach ($entity in @('instructor', 'curso', 'estudiante', 'inscripcion')) { $missing[$entity] = Find-MissingId -Entity $entity }

        $instructor = @{ Nombre = "$tag Instructor José"; Especialidad = 'Desarrollo de software'; Email = "$tag-instructor@example.com" }
        $instructorId = New-TestRecord -Entity instructor -Body $instructor -Name 'POST instructor'
        $student = @{ Nombre = "$tag Estudiante Lucía"; Email = "$tag-estudiante@example.com"; FechaNacimiento = '2002-05-15' }
        $studentId = New-TestRecord -Entity estudiante -Body $student -Name 'POST estudiante'
        $course = @{ Titulo = "$tag Curso uno"; Descripcion = 'Prueba de integración con acentos: programación.'; Nivel = 'Básico'; IdInstructor = $instructorId }
        $courseId = New-TestRecord -Entity curso -Body $course -Name 'POST curso con nivel Básico'
        $secondCourse = @{ Titulo = "$tag Curso dos"; Descripcion = 'Segundo curso temporal para comprobar duplicados al editar.'; Nivel = 'Intermedio'; IdInstructor = $instructorId }
        $secondCourseId = New-TestRecord -Entity curso -Body $secondCourse -Name 'POST curso con nivel Intermedio'
        $enrollment = @{ FechaInscripcion = '2026-01-15'; IdEstudiante = $studentId; IdCurso = $courseId }
        $enrollmentId = New-TestRecord -Entity inscripcion -Body $enrollment -Name 'POST inscripción'
        $secondEnrollment = @{ FechaInscripcion = '2026-01-15'; IdEstudiante = $studentId; IdCurso = $secondCourseId }
        $secondEnrollmentId = New-TestRecord -Entity inscripcion -Body $secondEnrollment -Name 'POST segunda inscripción en curso diferente'

        Assert-SavedFields -Entity instructor -Id $instructorId -Expected $instructor -Name 'Instructor creado'
        Assert-SavedFields -Entity estudiante -Id $studentId -Expected $student -Name 'Estudiante creado'
        $courseWithInstructor = $course.Clone()
        $courseWithInstructor.NombreInstructor = $instructor.Nombre
        Assert-SavedFields -Entity curso -Id $courseId -Expected $courseWithInstructor -Name 'Curso creado con instructor'
        Assert-SavedFields -Entity inscripcion -Id $enrollmentId -Expected $enrollment -Name 'Inscripción creada'

        foreach ($entry in @(
            @{ Entity = 'instructor'; Id = $instructorId },
            @{ Entity = 'estudiante'; Id = $studentId },
            @{ Entity = 'curso'; Id = $courseId },
            @{ Entity = 'inscripcion'; Id = $enrollmentId }
        )) {
            $result = Expect-Request -Method GET -Path "/api/$($entry.Entity)" -Status 200 -Name "Listar $($entry.Entity) después de crear"
            $matches = @($result.Json | Where-Object { [int](Get-Field -Object $_ -Name $idFields[$entry.Entity]) -eq $entry.Id })
            Assert-True -Condition ($matches.Count -eq 1) -Name "Listado contiene el $($entry.Entity) creado una sola vez"
            if ($entry.Entity -eq 'curso') {
                Assert-True -Condition ((Get-Field -Object $matches[0] -Name 'NombreInstructor') -ceq $instructor.Nombre) -Name 'Listado de cursos incluye nombre del instructor'
            }
        }

        $instructor.Nombre = "$tag Instructor José editado"
        $instructor.Especialidad = 'Arquitectura y bases de datos'
        $instructor.Email = "$tag-instructor-editado@example.com"
        $null = Expect-Request -Method PUT -Path "/api/instructor/$instructorId" -Status 204 -Name 'PUT instructor' -Body $instructor
        Assert-SavedFields -Entity instructor -Id $instructorId -Expected $instructor -Name 'Instructor actualizado'
        $student.Nombre = "$tag Estudiante Lucía editada"
        $student.Email = "$tag-estudiante-editado@example.com"
        $student.FechaNacimiento = '2001-06-20'
        $null = Expect-Request -Method PUT -Path "/api/estudiante/$studentId" -Status 204 -Name 'PUT estudiante' -Body $student
        Assert-SavedFields -Entity estudiante -Id $studentId -Expected $student -Name 'Estudiante actualizado'
        $course.Titulo = "$tag Curso uno editado"
        $course.Descripcion = 'Descripción actualizada y verificada.'
        $course.Nivel = 'Avanzado'
        $null = Expect-Request -Method PUT -Path "/api/curso/$courseId" -Status 204 -Name 'PUT curso con nivel Avanzado' -Body $course
        $courseWithInstructor = $course.Clone()
        $courseWithInstructor.NombreInstructor = $instructor.Nombre
        Assert-SavedFields -Entity curso -Id $courseId -Expected $courseWithInstructor -Name 'Curso actualizado y nombre del instructor actualizado'
        $enrollment.FechaInscripcion = '2026-02-16'
        $null = Expect-Request -Method PUT -Path "/api/inscripcion/$enrollmentId" -Status 204 -Name 'PUT inscripción conserva su propia combinación sin falso duplicado' -Body $enrollment
        Assert-SavedFields -Entity inscripcion -Id $enrollmentId -Expected $enrollment -Name 'Inscripción actualizada'

        $null = Expect-Request -Method POST -Path '/api/inscripcion' -Status 409 -Name 'Rechazar inscripción duplicada al crear' -Body $enrollment
        $duplicateUpdate = $secondEnrollment.Clone()
        $duplicateUpdate.IdCurso = $courseId
        $null = Expect-Request -Method PUT -Path "/api/inscripcion/$secondEnrollmentId" -Status 409 -Name 'Rechazar inscripción duplicada al editar' -Body $duplicateUpdate
        Assert-SavedFields -Entity inscripcion -Id $secondEnrollmentId -Expected $secondEnrollment -Name 'PUT duplicado conserva la segunda inscripción original'

        $invalidCourse = $course.Clone()
        $invalidCourse.Nivel = 'Experto'
        $null = Expect-Request -Method POST -Path '/api/curso' -Status 400 -Name 'Rechazar nivel inválido al crear' -Body $invalidCourse
        $null = Expect-Request -Method PUT -Path "/api/curso/$courseId" -Status 400 -Name 'Rechazar nivel inválido al editar' -Body $invalidCourse
        $invalidCourse = $course.Clone()
        $invalidCourse.IdInstructor = $missing.instructor
        $null = Expect-Request -Method POST -Path '/api/curso' -Status 400 -Name 'Rechazar curso con instructor inexistente' -Body $invalidCourse
        $null = Expect-Request -Method PUT -Path "/api/curso/$courseId" -Status 400 -Name 'Rechazar edición con instructor inexistente' -Body $invalidCourse
        Assert-SavedFields -Entity curso -Id $courseId -Expected $course -Name 'Validaciones fallidas conservan el curso'

        $invalidEnrollment = $enrollment.Clone()
        $invalidEnrollment.IdEstudiante = $missing.estudiante
        $null = Expect-Request -Method POST -Path '/api/inscripcion' -Status 400 -Name 'Rechazar inscripción con estudiante inexistente y curso válido' -Body $invalidEnrollment
        $null = Expect-Request -Method PUT -Path "/api/inscripcion/$enrollmentId" -Status 400 -Name 'Rechazar edición con estudiante inexistente' -Body $invalidEnrollment
        $invalidEnrollment = $enrollment.Clone()
        $invalidEnrollment.IdCurso = $missing.curso
        $null = Expect-Request -Method POST -Path '/api/inscripcion' -Status 400 -Name 'Rechazar inscripción con curso inexistente y estudiante válido' -Body $invalidEnrollment
        $null = Expect-Request -Method PUT -Path "/api/inscripcion/$enrollmentId" -Status 400 -Name 'Rechazar edición con curso inexistente' -Body $invalidEnrollment
        Assert-SavedFields -Entity inscripcion -Id $enrollmentId -Expected $enrollment -Name 'Validaciones fallidas conservan la inscripción'

        $invalidStudent = $student.Clone()
        $invalidStudent.FechaNacimiento = (Get-Date).AddDays(1).ToString('yyyy-MM-dd')
        $null = Expect-Request -Method POST -Path '/api/estudiante' -Status 400 -Name 'Rechazar fecha de nacimiento futura' -Body $invalidStudent
        $invalidInstructor = $instructor.Clone()
        $invalidInstructor.Email = 'correo-invalido'
        $null = Expect-Request -Method POST -Path '/api/instructor' -Status 400 -Name 'Rechazar correo inválido' -Body $invalidInstructor
        $invalidInstructor = $instructor.Clone()
        $invalidInstructor.Nombre = '   '
        $null = Expect-Request -Method POST -Path '/api/instructor' -Status 400 -Name 'Rechazar nombre vacío' -Body $invalidInstructor

        foreach ($entry in @(
            @{ Entity = 'instructor'; Body = $instructor },
            @{ Entity = 'estudiante'; Body = $student },
            @{ Entity = 'curso'; Body = $course },
            @{ Entity = 'inscripcion'; Body = $enrollment }
        )) {
            $null = Expect-Request -Method PUT -Path "/api/$($entry.Entity)/$($missing[$entry.Entity])" -Status 404 -Name "PUT $($entry.Entity) inexistente" -Body $entry.Body
        }
        $null = Expect-Request -Method DELETE -Path "/api/instructor/$instructorId" -Status 409 -Name 'Proteger instructor con cursos asociados'
        $null = Expect-Request -Method DELETE -Path "/api/estudiante/$studentId" -Status 409 -Name 'Proteger estudiante con inscripciones'
        $null = Expect-Request -Method DELETE -Path "/api/curso/$courseId" -Status 409 -Name 'Proteger curso con inscripciones'

        # El orden respeta las FK y cubre DELETE seguido de GET 404.
        foreach ($entity in @('inscripcion', 'curso', 'estudiante', 'instructor')) {
            foreach ($id in @($script:created[$entity].ToArray())) { Remove-TestRecord -Entity $entity -Id $id }
        }
    }
    else { Write-Host 'Para probar POST, PUT, DELETE y reglas de negocio: agrega -AllowWrites.' }
}
catch {
    Add-Check -Name $script:activeCheck -Passed $false -Detail $_.Exception.Message
}
finally {
    # Best effort: un error de limpieza no impide intentar los demás registros.
    foreach ($entity in @('inscripcion', 'curso', 'estudiante', 'instructor')) {
        foreach ($id in @($script:created[$entity].ToArray())) {
            try { Remove-TestRecord -Entity $entity -Id $id -Cleanup }
            catch { Add-Check -Name "Limpieza $entity/$id" -Passed $false -Detail $_.Exception.Message }
        }
    }
}

$passed = @($script:checks | Where-Object Correcta).Count
$failed = @($script:checks | Where-Object { -not $_.Correcta }).Count
$pending = @(
    foreach ($entity in @('inscripcion', 'curso', 'estudiante', 'instructor')) {
        foreach ($id in $script:created[$entity]) { "$entity/$id" }
    }
)
$elapsed = [math]::Round(((Get-Date) - $started).TotalSeconds, 1)
Write-Host "`nResultado: $passed correctas, $failed fallidas. Duración: $elapsed s."
if ($pending.Count -gt 0) {
    Write-Host "Registros temporales pendientes de limpiar: $($pending -join ', ')" -ForegroundColor Yellow
}
elseif ($AllowWrites) { Write-Host 'Limpieza completa: no quedan IDs temporales registrados por esta ejecución.' }
if ($failed -gt 0 -or $pending.Count -gt 0) { exit 1 }
exit 0
