namespace IdeaVault.Core.Entities;

public class Category
{
    /// <summary>Well-known id of the "Caixa de entrada" (inbox) category, target of quick captures.</summary>
    public static readonly Guid InboxId = Guid.Parse("00000000-0000-0000-0000-000000000001");

    public Guid Id { get; set; }
    public required string Name { get; set; }
    public required string Emoji { get; set; }

    /// <summary>Hex color, e.g. <c>#7C5CFF</c>.</summary>
    public required string Color { get; set; }

    public int SortOrder { get; set; }

    /// <summary>Section titles automatically created for every new idea in this category.</summary>
    public List<string> TemplateSections { get; set; } = [];

    public List<Idea> Ideas { get; set; } = [];

    public bool IsInbox => Id == InboxId;
}
