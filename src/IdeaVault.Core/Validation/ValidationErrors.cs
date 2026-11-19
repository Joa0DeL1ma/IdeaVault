using IdeaVault.Core.Exceptions;

namespace IdeaVault.Core.Validation;

/// <summary>Collects field errors and throws a single <see cref="ValidationException"/> with all of them.</summary>
internal sealed class ValidationErrors
{
    private readonly Dictionary<string, List<string>> _errors = [];

    public ValidationErrors When(bool invalid, string field, string message)
    {
        if (invalid)
        {
            if (!_errors.TryGetValue(field, out var messages))
            {
                _errors[field] = messages = [];
            }

            messages.Add(message);
        }

        return this;
    }

    public ValidationErrors MaxLength(string? value, int max, string field) =>
        When(value is not null && value.Length > max, field, $"Must have at most {max} characters.");

    public ValidationErrors Required(string? value, string field) =>
        When(string.IsNullOrWhiteSpace(value), field, "Is required.");

    public void ThrowIfAny()
    {
        if (_errors.Count > 0)
        {
            throw new ValidationException(_errors.ToDictionary(e => e.Key, e => e.Value.ToArray()));
        }
    }
}
