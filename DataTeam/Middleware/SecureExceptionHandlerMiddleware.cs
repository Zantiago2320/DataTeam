using System.Net;
using System.Text.Json;

namespace DataTeam.Middleware;

/// <summary>
/// Middleware para manejo seguro de excepciones sin exponer información sensible
/// </summary>
public class SecureExceptionHandlerMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<SecureExceptionHandlerMiddleware> _logger;
    private readonly IWebHostEnvironment _environment;

    public SecureExceptionHandlerMiddleware(
        RequestDelegate next,
        ILogger<SecureExceptionHandlerMiddleware> logger,
        IWebHostEnvironment environment)
    {
        _next = next;
        _logger = logger;
        _environment = environment;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        // Log detallado solo en servidor (NO enviar al cliente)
        _logger.LogError(exception, 
            "Error no controlado en {Path}. Usuario: {User}",
            context.Request.Path,
            context.User?.Identity?.Name ?? "Anónimo");

        // Determinar código de estado HTTP
        var statusCode = exception switch
        {
            ArgumentException => HttpStatusCode.BadRequest,
            UnauthorizedAccessException => HttpStatusCode.Forbidden,
            KeyNotFoundException => HttpStatusCode.NotFound,
            InvalidOperationException => HttpStatusCode.BadRequest,
            _ => HttpStatusCode.InternalServerError
        };

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)statusCode;

        // SEGURIDAD: NO exponer detalles técnicos en producción
        object response;

        if (_environment.IsDevelopment())
        {
            response = new
            {
                error = "Error en la aplicación",
                message = exception.Message,
                type = exception.GetType().Name,
                statusCode = (int)statusCode,
                stackTrace = exception.StackTrace ?? string.Empty
            };
        }
        else
        {
            response = new
            {
                error = "Error en la aplicación",
                message = GetSafeErrorMessage(exception),
                type = exception.GetType().Name,
                statusCode = (int)statusCode,
                stackTrace = string.Empty
            };
        }

        var jsonResponse = JsonSerializer.Serialize(response, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true
        });

        return context.Response.WriteAsync(jsonResponse);
    }

    private static string GetSafeErrorMessage(Exception exception)
    {
        // Mensajes genéricos seguros para producción
        return exception switch
        {
            ArgumentException => "Los datos proporcionados no son válidos",
            UnauthorizedAccessException => "No tiene permisos para realizar esta acción",
            KeyNotFoundException => "El recurso solicitado no existe",
            InvalidOperationException => "La operación no se pudo completar",
            _ => "Ocurrió un error inesperado. Por favor, contacte al administrador"
        };
    }
}
