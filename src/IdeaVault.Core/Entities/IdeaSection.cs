namespace IdeaVault.Core.Entities;

/// <summary>A titled block of markdown inside an idea (e.g. "Core mechanic", "Target audience").</summary>
public class IdeaSection
{
    public Guid Id { get; set; }
    public Guid IdeaId { get; set; }
    public required string Title { get; set; }
    public string Content { get; set; } = string.Empty;
    public int SortOrder { get; set; }
}
