using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace IdeaVault.Api.Auth;

public sealed record LoginRequest(string Password);

public sealed record LoginResponse(string Token, DateTime ExpiresAt);

public sealed class TokenService(IOptions<AuthOptions> options, TimeProvider clock)
{
    public LoginResponse CreateToken()
    {
        var settings = options.Value;
        var now = clock.GetUtcNow().UtcDateTime;
        var expiresAt = now.AddDays(settings.TokenLifetimeDays);

        var token = new JsonWebTokenHandler().CreateToken(new SecurityTokenDescriptor
        {
            Issuer = AuthOptions.Issuer,
            Audience = AuthOptions.Issuer,
            Subject = new ClaimsIdentity([new Claim(JwtRegisteredClaimNames.Sub, "owner")]),
            IssuedAt = now,
            NotBefore = now,
            Expires = expiresAt,
            SigningCredentials = new SigningCredentials(CreateSigningKey(settings.JwtSecret), SecurityAlgorithms.HmacSha256),
        });

        return new LoginResponse(token, expiresAt);
    }

    public static SymmetricSecurityKey CreateSigningKey(string secret) => new(Encoding.UTF8.GetBytes(secret));
}
