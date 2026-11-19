using IdeaVault.Core.Entities;

namespace IdeaVault.Web.Services;

/// <summary>Portuguese (pt-BR) labels for domain values.</summary>
public static class Labels
{
    public static string Status(IdeaStatus status) => status switch
    {
        IdeaStatus.Seed => "🌱 Semente",
        IdeaStatus.Growing => "🌿 Crescendo",
        IdeaStatus.Executing => "🚀 Executando",
        IdeaStatus.Archived => "📦 Arquivada",
        _ => status.ToString(),
    };

    public static string Stars(int excitement) =>
        new string('★', excitement) + new string('☆', Idea.MaxExcitement - excitement);

    public static string Relative(DateTime utc)
    {
        var delta = DateTime.UtcNow - utc;
        return delta.TotalMinutes switch
        {
            < 1 => "agora",
            < 60 => $"há {(int)delta.TotalMinutes} min",
            < 60 * 24 => $"há {(int)delta.TotalHours} h",
            < 60 * 24 * 30 => $"há {(int)delta.TotalDays} d",
            _ => utc.ToLocalTime().ToString("dd/MM/yyyy"),
        };
    }
}
