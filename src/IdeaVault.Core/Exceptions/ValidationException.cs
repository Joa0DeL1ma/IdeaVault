namespace IdeaVault.Core.Exceptions;

public sealed class ValidationException(IReadOnlyDictionary<string, string[]> errors)
    : Exception("One or more validation errors occurred.")
{
    /// <summary>Error messages keyed by field name.</summary>
    public IReadOnlyDictionary<string, string[]> Errors { get; } = errors;
}
