using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;

namespace Slotsi.Api.Infrastructure;

public class MongoExceptionHandler : IExceptionHandler
{
    private readonly ILogger<MongoExceptionHandler> _logger;

    public MongoExceptionHandler(ILogger<MongoExceptionHandler> logger)
        => _logger = logger;

    public async ValueTask<bool> TryHandleAsync(
        HttpContext contexto,
        Exception excepcion,
        CancellationToken ct)
    {
        if (excepcion is not (MongoException or TimeoutException))
            return false;

        _logger.LogError(excepcion, "Error de acceso a MongoDB");

        contexto.Response.StatusCode = StatusCodes.Status503ServiceUnavailable;
        await contexto.Response.WriteAsJsonAsync(new ProblemDetails
        {
            Status = StatusCodes.Status503ServiceUnavailable,
            Title = "Base de datos no disponible",
            Detail = "No se pudo conectar con MongoDB. Inténtalo de nuevo más tarde."
        }, ct);

        return true;
    }
}