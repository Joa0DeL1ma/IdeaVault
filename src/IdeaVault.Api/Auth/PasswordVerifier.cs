using System.Security.Cryptography;
using System.Text;

namespace IdeaVault.Api.Auth;

public static class PasswordVerifier
{
    /// <summary>
    /// Compares in constant time. Both values are hashed first so that inputs of different lengths
    /// also take the same time (FixedTimeEquals alone returns early on a length mismatch).
    /// </summary>
    public static bool Matches(string? attempt, string expected)
    {
        var attemptHash = SHA256.HashData(Encoding.UTF8.GetBytes(attempt ?? string.Empty));
        var expectedHash = SHA256.HashData(Encoding.UTF8.GetBytes(expected));
        return CryptographicOperations.FixedTimeEquals(attemptHash, expectedHash);
    }
}
