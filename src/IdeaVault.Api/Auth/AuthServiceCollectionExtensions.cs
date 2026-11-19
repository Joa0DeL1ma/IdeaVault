using System.Threading.RateLimiting;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace IdeaVault.Api.Auth;

public static class AuthServiceCollectionExtensions
{
    public const string LoginRateLimitPolicy = "login";

    public static IServiceCollection AddSingleUserAuth(this IServiceCollection services)
    {
        services.AddOptions<AuthOptions>()
            .Configure<IConfiguration>((options, config) =>
            {
                options.Password = config["APP_PASSWORD"] ?? string.Empty;
                options.JwtSecret = config["JWT_SECRET"] ?? string.Empty;
            })
            .ValidateDataAnnotations()
            .ValidateOnStart(); // fail fast on a misconfigured deploy

        services.AddSingleton<TokenService>();

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer();
        services.AddOptions<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme)
            .Configure<IOptions<AuthOptions>>((jwt, auth) =>
            {
                jwt.MapInboundClaims = false;
                jwt.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidIssuer = AuthOptions.Issuer,
                    ValidAudience = AuthOptions.Issuer,
                    IssuerSigningKey = TokenService.CreateSigningKey(auth.Value.JwtSecret),
                    ClockSkew = TimeSpan.FromMinutes(1),
                };
            });

        services.AddAuthorization();

        // Brute-force protection: there is a single password, so a global limit is enough.
        services.AddRateLimiter(limiter =>
        {
            limiter.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
            limiter.AddFixedWindowLimiter(LoginRateLimitPolicy, window =>
            {
                window.PermitLimit = 5;
                window.Window = TimeSpan.FromMinutes(1);
                window.QueueLimit = 0;
                window.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
            });
        });

        return services;
    }
}
