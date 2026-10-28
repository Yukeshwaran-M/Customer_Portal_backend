using CustomerOperations.Api.Dtos;
using CustomerOperations.Api.Exceptions;

namespace CustomerOperations.Api.Middleware;

/// <summary>One consistent error shape for every failure: { "message": "...", "code": "..." }.</summary>
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
        catch (OperationCanceledException) when (context.RequestAborted.IsCancellationRequested)
        {
            // The browser cancelled the request (e.g. switchMap on a newer search). Nothing to report.
            _logger.LogDebug("Request cancelled by the client: {Path}", context.Request.Path);
        }
        catch (ApiException ex)
        {
            await WriteAsync(context, ex.StatusCode, new ApiErrorResponse(ex.Message, ex.Code));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception for {Method} {Path}", context.Request.Method, context.Request.Path);
            await WriteAsync(context, StatusCodes.Status500InternalServerError,
                new ApiErrorResponse("An unexpected error occurred. Please try again.", ErrorCodes.ServerError));
        }
    }

    private static Task WriteAsync(HttpContext context, int statusCode, ApiErrorResponse body)
    {
        if (context.Response.HasStarted)
        {
            return Task.CompletedTask;
        }

        context.Response.StatusCode = statusCode;
        return context.Response.WriteAsJsonAsync(body);
    }
}
