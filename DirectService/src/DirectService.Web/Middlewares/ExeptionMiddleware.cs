using Shared;
using Shared.Exceptions;

namespace DirectService.Web.Middlewares;

public class ExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionMiddleware> _logger;

    public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }
    
    public async Task InvokeAsync(HttpContext httpContext)
    {
        try
        {
            await _next(httpContext);
        }
        catch (Exception e)
        {
            await HandleExceptionAsync(httpContext, e);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        _logger.LogError(exception, exception.Message);

        (int statusCode, Error error) = exception switch
        {
            BadRequestException ex => (StatusCodes.Status400BadRequest, ex.Error),
            
            ConflictException ex => (StatusCodes.Status409Conflict, ex.Error),
            
            FailureException ex => (StatusCodes.Status500InternalServerError, ex.Error),
            
            NotFoundException ex => (StatusCodes.Status404NotFound, ex.Error),
            
            ValidationException ex => (StatusCodes.Status400BadRequest, ex.Error),

            _ => (StatusCodes.Status500InternalServerError, Error.Failure("server.internal", exception.Message))
        };
        var envelope = Envelope.Error(error);
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = statusCode;

        await context.Response.WriteAsJsonAsync(envelope);
    }
}

public static class ExceptionMiddlewareExtensions
{
    public static IApplicationBuilder UseExceptionMiddleware(this WebApplication builder)
    {
        return builder.UseMiddleware<ExceptionMiddleware>();
    }
}