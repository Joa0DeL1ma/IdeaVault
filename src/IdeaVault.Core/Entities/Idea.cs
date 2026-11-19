namespace IdeaVault.Core.Entities;

public class Idea
{
    public const int MinExcitement = 1;
    public const int MaxExcitement = 5;

    public Guid Id { get; set; }
    public required string Title { get; set; }
    public string Summary { get; set; } = string.Empty;

    public Guid CategoryId { get; set; }
    public Category? Category { get; set; }

    public IdeaStatus Status { get; set; } = IdeaStatus.Seed;

    /// <summary>How excited I am about the idea, from 1 to 5.</summary>
    public int Excitement { get; set; } = 3;

    public List<string> Tags { get; set; } = [];
    public bool IsPinned { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public List<IdeaSection> Sections { get; set; } = [];
}
