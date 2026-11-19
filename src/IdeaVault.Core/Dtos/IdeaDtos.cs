using IdeaVault.Core.Entities;

namespace IdeaVault.Core.Dtos;

/// <summary>Lightweight projection used by the idea list / cards.</summary>
public sealed record IdeaSummaryDto(
    Guid Id,
    string Title,
    string Summary,
    Guid CategoryId,
    string CategoryName,
    string CategoryEmoji,
    string CategoryColor,
    IdeaStatus Status,
    int Excitement,
    IReadOnlyList<string> Tags,
    bool IsPinned,
    DateTime CreatedAt,
    DateTime UpdatedAt);

public sealed record IdeaDetailDto(
    Guid Id,
    string Title,
    string Summary,
    Guid CategoryId,
    string CategoryName,
    string CategoryEmoji,
    string CategoryColor,
    IdeaStatus Status,
    int Excitement,
    IReadOnlyList<string> Tags,
    bool IsPinned,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    IReadOnlyList<IdeaSectionDto> Sections);

public sealed record CreateIdeaRequest(
    string Title,
    Guid CategoryId,
    string? Summary = null,
    IdeaStatus Status = IdeaStatus.Seed,
    int Excitement = 3,
    IReadOnlyList<string>? Tags = null,
    bool IsPinned = false);

/// <summary>Full replacement of the editable fields of an idea (sections are edited separately).</summary>
public sealed record UpdateIdeaRequest(
    string Title,
    Guid CategoryId,
    string? Summary,
    IdeaStatus Status,
    int Excitement,
    IReadOnlyList<string>? Tags,
    bool IsPinned);

/// <summary>Free text captured in a hurry; always lands in the inbox.</summary>
public sealed record QuickCaptureRequest(string Text);

public enum IdeaSort
{
    /// <summary>Most recently updated first.</summary>
    Updated,

    /// <summary>Most exciting first, ties broken by most recently updated.</summary>
    Excitement,
}

/// <summary>Filters for the idea list. Pinned ideas always come first.</summary>
public sealed record IdeaQuery(
    Guid? CategoryId = null,
    IdeaStatus? Status = null,
    string? Tag = null,
    string? Search = null,
    IdeaSort Sort = IdeaSort.Updated);
