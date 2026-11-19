namespace IdeaVault.Core.Dtos;

public sealed record IdeaSectionDto(Guid Id, string Title, string Content, int SortOrder);

public sealed record SaveSectionRequest(string Title, string? Content = null);

/// <summary>All section ids of an idea, in the desired order.</summary>
public sealed record ReorderSectionsRequest(IReadOnlyList<Guid> SectionIds);
