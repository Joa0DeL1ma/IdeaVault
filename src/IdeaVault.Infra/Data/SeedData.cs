using IdeaVault.Core.Entities;

namespace IdeaVault.Infra.Data;

/// <summary>Default categories (with section templates) inserted on first startup.</summary>
internal static class SeedData
{
    // A method (not a static field) so every context gets fresh, untracked instances.
    public static Category[] CreateCategories() =>
    [
        new()
        {
            Id = Category.InboxId,
            Name = "Caixa de entrada",
            Emoji = "📥",
            Color = "#8B93A7",
            SortOrder = 0,
            TemplateSections = [],
        },
        new()
        {
            Id = Guid.Parse("00000000-0000-0000-0000-000000000002"),
            Name = "Jogos",
            Emoji = "🎮",
            Color = "#7C5CFF",
            SortOrder = 1,
            TemplateSections = ["Mecânica central", "Gênero", "Referências", "Escopo/MVP", "Próximo passo"],
        },
        new()
        {
            Id = Guid.Parse("00000000-0000-0000-0000-000000000003"),
            Name = "Negócios",
            Emoji = "💼",
            Color = "#22C55E",
            SortOrder = 2,
            TemplateSections = ["Problema", "Público", "Como ganha dinheiro", "Concorrentes", "Próximo passo"],
        },
        new()
        {
            Id = Guid.Parse("00000000-0000-0000-0000-000000000004"),
            Name = "Histórias",
            Emoji = "📖",
            Color = "#F59E0B",
            SortOrder = 3,
            TemplateSections = ["Premissa", "Personagens", "Mundo", "Conflito", "Final"],
        },
        new()
        {
            Id = Guid.Parse("00000000-0000-0000-0000-000000000005"),
            Name = "Tecnologia/Pesquisa",
            Emoji = "🔬",
            Color = "#38BDF8",
            SortOrder = 4,
            TemplateSections = ["Problema", "Hipótese", "Como testar", "Referências", "Próximo passo"],
        },
    ];
}
