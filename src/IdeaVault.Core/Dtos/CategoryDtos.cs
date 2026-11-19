namespace IdeaVault.Core.Dtos;

public sealed record CategoryDto(
    Guid Id,
    string Name,
    string Emoji,
    string Color,
    int SortOrder,
    IReadOnlyList<string> TemplateSections,
    int IdeaCount,
    bool IsInbox);

/// <summary>Payload used both to create and to update a category.</summary>
public sealed record SaveCategoryRequest(
    string Name,
    string Emoji,
    string Color,
    int SortOrder = 0,
    IReadOnlyList<string>? TemplateSections = null);
