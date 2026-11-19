using IdeaVault.Api.Auth;

namespace IdeaVault.Tests;

public sealed class PasswordVerifierTests
{
    [Fact]
    public void Matches_the_exact_password() =>
        Assert.True(PasswordVerifier.Matches("correct horse battery", "correct horse battery"));

    [Theory]
    [InlineData("correct horse batter")]   // prefix
    [InlineData("correct horse battery ")] // longer
    [InlineData("Correct horse battery")]  // case-sensitive
    [InlineData("")]
    [InlineData(null)]
    public void Rejects_anything_else(string? attempt) =>
        Assert.False(PasswordVerifier.Matches(attempt, "correct horse battery"));
}
