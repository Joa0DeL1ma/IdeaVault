using IdeaVault.Api.Auth;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Extensions.Options;

namespace IdeaVault.Api.Endpoints;

public static class AuthEndpoints
{
    public static IEndpointRouteBuilder MapAuthEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapPost("/api/auth/login", Login)
            .WithTags("Auth")
            .WithSummary("Exchanges the app password for a JWT.")
            .AllowAnonymous()
            .RequireRateLimiting(AuthServiceCollectionExtensions.LoginRateLimitPolicy);

        return app;
    }

    private static Results<Ok<LoginResponse>, ProblemHttpResult> Login(
        LoginRequest request, IOptions<AuthOptions> options, TokenService tokens) =>
        PasswordVerifier.Matches(request.Password, options.Value.Password)
            ? TypedResults.Ok(tokens.CreateToken())
            : TypedResults.Problem(statusCode: StatusCodes.Status401Unauthorized, title: "Invalid password");
}
