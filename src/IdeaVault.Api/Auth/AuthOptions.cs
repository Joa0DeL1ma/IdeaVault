using System.ComponentModel.DataAnnotations;

namespace IdeaVault.Api.Auth;

/// <summary>Single-user auth settings, read from the APP_PASSWORD and JWT_SECRET environment variables.</summary>
public sealed class AuthOptions
{
    public const string Issuer = "ideavault";

    [Required(ErrorMessage = "APP_PASSWORD is not configured.")]
    [MinLength(8, ErrorMessage = "APP_PASSWORD must have at least 8 characters.")]
    public string Password { get; set; } = string.Empty;

    [Required(ErrorMessage = "JWT_SECRET is not configured.")]
    [MinLength(32, ErrorMessage = "JWT_SECRET must have at least 32 characters.")]
    public string JwtSecret { get; set; } = string.Empty;

    [Range(1, 365)]
    public int TokenLifetimeDays { get; set; } = 30;
}
