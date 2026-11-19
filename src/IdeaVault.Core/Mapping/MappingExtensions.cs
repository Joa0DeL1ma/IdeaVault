using IdeaVault.Core.Dtos;
using IdeaVault.Core.Entities;

namespace IdeaVault.Core.Mapping;

/// <summary>Entity → DTO mapping. Entities never leave the application layer.</summary>
public static class MappingExtensions
{
    public static CategoryDto ToDto(this Category c, int ideaCount = 0) =>
        new(c.Id, c.Name, c.Emoji, c.Color, c.SortOrder, c.TemplateSections, ideaCount, c.IsInbox);

    public static IdeaSectionDto ToDto(this IdeaSection s) =>
        new(s.Id, s.Title, s.Content, s.SortOrder);

    public static IdeaSummaryDto ToSummaryDto(this Idea i)
    {
        var category = RequireCategory(i);
        return new(i.Id, i.Title, i.Summary, i.CategoryId, category.Name, category.Emoji, category.Color,
            i.Status, i.Excitement, i.Tags, i.IsPinned, i.CreatedAt, i.UpdatedAt);
    }

    public static IdeaDetailDto ToDetailDto(this Idea i)
    {
        var category = RequireCategory(i);
        return new(i.Id, i.Title, i.Summary, i.CategoryId, category.Name, category.Emoji, category.Color,
            i.Status, i.Excitement, i.Tags, i.IsPinned, i.CreatedAt, i.UpdatedAt,
            [.. i.Sections.OrderBy(s => s.SortOrder).Select(s => s.ToDto())]);
    }

    private static Category RequireCategory(Idea idea) =>
        idea.Category ?? throw new InvalidOperationException($"Category of idea '{idea.Id}' was not loaded.");
}
