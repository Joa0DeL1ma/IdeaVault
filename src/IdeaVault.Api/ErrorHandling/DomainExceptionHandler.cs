using IdeaVault.Core.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace IdeaVault.Api.ErrorHandling;

/// <summary>Translates domain and bad-request exceptions into RFC 9457 ProblemDetails responses.</summary>
internal sealed class DomainExceptionHandler(IProblemDetailsService problemDetails) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext context, Exception exception, CancellationToken ct)
    {
        ProblemDetails? problem = exception switch
        {
            ValidationException e => new HttpValidationProblemDetails(e.Errors.ToDictionary())
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "Validation failed",
            },
            NotFoundException e => new ProblemDetails
            {
                Status = StatusCodes.Status404NotFound,
                Title = "Resource not found",
                Detail = e.Message,
            },
            ConflictException e => new ProblemDetails
            {
                Status = StatusCodes.Status409Conflict,
                Title = "Conflict",
                Detail = e.Message,
            },
            // Malformed JSON or un-bindable route/query values (e.g. ?status=Banana).
            BadHttpRequestException e => new ProblemDetails
            {
                Status = e.StatusCode,
                Title = "Bad request",
                Detail = e.Message,
            },
            _ => null, // unexpected: let the default handler return a generic 500
        };

        if (problem is null)
        {
            return false;
        }

        context.Response.StatusCode = problem.Status!.Value;
        return await problemDetails.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = context,
            ProblemDetails = problem,
            Exception = exception,
        });
    }
}
