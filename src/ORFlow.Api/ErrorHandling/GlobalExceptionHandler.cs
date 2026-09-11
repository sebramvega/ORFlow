using Microsoft.AspNetCore.Diagnostics;

namespace ORFlow.Api.ErrorHandling;

public sealed class GlobalExceptionHandler : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        if (exception is ArgumentException)
        {
            await Results.Problem(
                statusCode: StatusCodes.Status400BadRequest,
                title: "Invalid request",
                detail: exception.Message)
                .ExecuteAsync(httpContext);

            return true;
        }

        if (exception is InvalidOperationException)
        {
            await Results.Problem(
                statusCode: StatusCodes.Status409Conflict,
                title: "Invalid operation",
                detail: exception.Message)
                .ExecuteAsync(httpContext);

            return true;
        }

        await Results.Problem(
            statusCode: StatusCodes.Status500InternalServerError,
            title: "An unexpected error occurred")
            .ExecuteAsync(httpContext);

        return true;
    }
}
