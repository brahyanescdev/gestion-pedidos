using System.Net;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Pedidos.Application.Exceptions;
using Pedidos.Domain.Exceptions;

namespace Pedidos.Api.Middleware;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (NotFoundException ex)
        {
            await WriteProblemAsync(context, HttpStatusCode.NotFound, ex.Message);
        }
        catch (DomainException ex)
        {
            await WriteProblemAsync(context, HttpStatusCode.BadRequest, ex.Message);
        }
        catch (DbUpdateConcurrencyException ex)
        {
            _logger.LogWarning(ex, "Conflicto de concurrencia al guardar cambios.");
            await WriteProblemAsync(context, HttpStatusCode.Conflict, "El registro fue modificado por otra operación, intente de nuevo.");
        }
        catch (DbUpdateException ex)
        {
            _logger.LogWarning(ex, "Error al guardar cambios en la base de datos.");
            await WriteProblemAsync(context, HttpStatusCode.Conflict, "No se pudo guardar el cambio: verifique que los datos no dupliquen un registro existente.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error no controlado procesando la solicitud.");
            await WriteProblemAsync(context, HttpStatusCode.InternalServerError, "Ocurrió un error inesperado.");
        }
    }

    private static Task WriteProblemAsync(HttpContext context, HttpStatusCode statusCode, string message)
    {
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)statusCode;

        var payload = JsonSerializer.Serialize(new { error = message });
        return context.Response.WriteAsync(payload);
    }
}
