using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerUI;
using VL221407Desafio2.API.Documentation;
using VL221407Desafio2.API.Infrastructure;
using VL221407Desafio2.BL;
using VL221407Desafio2.Common;
using VL221407Desafio2.DAL;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers().ConfigureApiBehaviorOptions(options =>
{
    options.InvalidModelStateResponseFactory = context =>
    {
        var details = new ValidationProblemDetails(context.ModelState)
        {
            Status = 400,
            Title = "Revisa los datos de la solicitud",
            Detail = "Corrige los campos indicados e inténtalo de nuevo.",
            Instance = context.HttpContext.Request.Path
        };
        details.Extensions["traceId"] = context.HttpContext.TraceIdentifier;
        return new BadRequestObjectResult(details) { ContentTypes = { "application/problem+json" } };
    };
});
builder.Services.AddProblemDetails(options => options.CustomizeProblemDetails = context =>
{
    context.ProblemDetails.Extensions["traceId"] = context.HttpContext.TraceIdentifier;
});
builder.Services.AddExceptionHandler<ApiExceptionHandler>();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Aula · Gestión académica",
        Version = "v1",
        Description = "API de VL221407Desafio2. Administra instructores, cursos, estudiantes e inscripciones. " +
            "Crea primero un instructor y un estudiante, luego un curso y finalmente su inscripción. " +
            "Usa los identificadores devueltos por POST. Las respuestas de error incluyen detalle e identificador de solicitud."
    });
    options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, "VL221407Desafio2.API.xml"));
    options.SchemaFilter<EjemplosSchemaFilter>();
    options.CustomOperationIds(api => api.ActionDescriptor.RouteValues["controller"] + "_" + api.ActionDescriptor.RouteValues["action"]);
});
builder.Services.AddSingleton(new ConexionSql(builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Falta ConnectionStrings:DefaultConnection.")));
builder.Services.AddSingleton(new CalendarioAcademico(TimeProvider.System,
    builder.Configuration["Aplicacion:ZonaHoraria"] ?? "America/El_Salvador"));
builder.Services.AddScoped(typeof(IRepositorio<>), typeof(Repositorio<>));
builder.Services.AddScoped<IInscripcionRepositorio, InscripcionRepositorio>();
builder.Services.AddAutoMapper(config =>
{
    var key = builder.Configuration["AutoMapper:LicenseKey"];
    if (!string.IsNullOrWhiteSpace(key)) config.LicenseKey = key;
}, typeof(MapeoPerfil));
builder.Services.AddScoped<InstructorServicio>();
builder.Services.AddScoped<CursoServicio>();
builder.Services.AddScoped<EstudianteServicio>();
builder.Services.AddScoped<InscripcionServicio>();

var app = builder.Build();
app.Services.GetRequiredService<IMapper>().ConfigurationProvider.AssertConfigurationIsValid();
app.UseExceptionHandler();
app.UseStatusCodePages();
app.Use(async (context, next) =>
{
    context.Response.Headers["X-Content-Type-Options"] = "nosniff";
    context.Response.Headers["Referrer-Policy"] = "same-origin";
    context.Response.Headers["X-Frame-Options"] = "SAMEORIGIN";
    await next();
});
app.UseDefaultFiles();
app.UseStaticFiles(new StaticFileOptions
{
    OnPrepareResponse = context =>
    {
        var contentType = context.Context.Response.ContentType;
        if (contentType is not null &&
            (contentType.StartsWith("text/", StringComparison.OrdinalIgnoreCase) ||
             contentType.StartsWith("application/javascript", StringComparison.OrdinalIgnoreCase) ||
             contentType.StartsWith("application/json", StringComparison.OrdinalIgnoreCase)) &&
            !contentType.Contains("charset=", StringComparison.OrdinalIgnoreCase))
        {
            context.Context.Response.ContentType = $"{contentType}; charset=utf-8";
        }
    }
});

if (app.Environment.IsDevelopment() || app.Configuration.GetValue<bool>("Swagger:Enabled"))
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "VL221407Desafio2 · v1");
        options.DocumentTitle = "Aula | Documentación API";
        options.InjectStylesheet("/swagger-custom.css");
        options.InjectJavascript("/swagger-custom.js");
        options.EnableFilter();
        options.EnableDeepLinking();
        options.DisplayRequestDuration();
        options.DefaultModelsExpandDepth(-1);
        options.DocExpansion(DocExpansion.List);
    });
}
app.MapGet("/health", () => Results.Ok(new { estado = "Disponible", servicio = "VL221407Desafio2.API" })).ExcludeFromDescription();
app.MapGet("/health/live", () => Results.Ok(new { estado = "Disponible", servicio = "VL221407Desafio2.API" })).ExcludeFromDescription();
app.MapGet("/health/ready", async (ConexionSql conexion, ILoggerFactory loggerFactory, CancellationToken ct) =>
{
    using var timeout = CancellationTokenSource.CreateLinkedTokenSource(ct);
    timeout.CancelAfter(TimeSpan.FromSeconds(5));
    try
    {
        await conexion.ComprobarAsync(timeout.Token);
        return Results.Ok(new { estado = "Disponible", baseDatos = "Disponible" });
    }
    catch (Exception error) when (!ct.IsCancellationRequested)
    {
        loggerFactory.CreateLogger("Readiness").LogWarning("Base de datos no disponible: {ErrorType}", error.GetType().Name);
        return Results.Json(new { estado = "No disponible", baseDatos = "No disponible" }, statusCode: 503);
    }
}).ExcludeFromDescription();
app.MapControllers();
app.Run();
