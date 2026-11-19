namespace IdeaVault.Web.Services;

/// <summary>A non-success API response, carrying the ProblemDetails title/detail and field errors.</summary>
public sealed class ApiException(int status, string message, IReadOnlyDictionary<string, string[]>? errors = null)
    : Exception(message)
{
    public int Status { get; } = status;
    public IReadOnlyDictionary<string, string[]> Errors { get; } = errors ?? new Dictionary<string, string[]>();
}
