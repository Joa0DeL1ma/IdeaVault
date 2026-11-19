namespace IdeaVault.Core.Exceptions;

/// <summary>The request is valid but conflicts with the current state (e.g. deleting a category in use).</summary>
public sealed class ConflictException(string message) : Exception(message);
