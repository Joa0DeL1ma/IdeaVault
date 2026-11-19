using IdeaVault.Core.Dtos;
using IdeaVault.Core.Entities;
using IdeaVault.Core.Exceptions;
using IdeaVault.Core.Interfaces;
using IdeaVault.Core.Mapping;
using IdeaVault.Core.Validation;

namespace IdeaVault.Core.Services;

public sealed class IdeaService(
    IIdeaRepository ideas,
    ICategoryRepository categories,
    TimeProvider clock) : IIdeaService
{
    public const int MaxTitleLength = 200;
    public const int MaxSummaryLength = 2000;
    public const int MaxTags = 10;
    public const int MaxTagLength = 30;

    public async Task<IReadOnlyList<IdeaSummaryDto>> SearchAsync(IdeaQuery query, CancellationToken ct = default)
    {
        var normalized = query with
        {
            Tag = string.IsNullOrWhiteSpace(query.Tag) ? null : NormalizeTag(query.Tag),
            Search = string.IsNullOrWhiteSpace(query.Search) ? null : query.Search.Trim(),
        };

        var result = await ideas.SearchAsync(normalized, ct);
        return [.. result.Select(i => i.ToSummaryDto())];
    }

    public async Task<IdeaDetailDto> GetAsync(Guid id, CancellationToken ct = default) =>
        (await FindAsync(id, ct)).ToDetailDto();

    public async Task<IdeaDetailDto> CreateAsync(CreateIdeaRequest request, CancellationToken ct = default)
    {
        Validate(request.Title, request.Summary, request.Status, request.Excitement, request.Tags);
        var category = await FindCategoryAsync(request.CategoryId, ct);

        var now = clock.GetUtcNow().UtcDateTime;
        var idea = new Idea
        {
            Title = request.Title.Trim(),
            Summary = request.Summary?.Trim() ?? string.Empty,
            CategoryId = category.Id,
            Category = category,
            Status = request.Status,
            Excitement = request.Excitement,
            Tags = NormalizeTags(request.Tags),
            IsPinned = request.IsPinned,
            CreatedAt = now,
            UpdatedAt = now,
            // The category template becomes empty sections, ready to be filled in.
            Sections = [.. category.TemplateSections.Select((title, index) =>
                new IdeaSection { Title = title, SortOrder = index })],
        };

        ideas.Add(idea);
        await ideas.SaveChangesAsync(ct);

        return idea.ToDetailDto();
    }

    public Task<IdeaDetailDto> QuickCaptureAsync(QuickCaptureRequest request, CancellationToken ct = default)
    {
        var text = request.Text?.Trim() ?? string.Empty;
        new ValidationErrors().Required(text, nameof(request.Text)).ThrowIfAny();

        var (title, summary) = SplitCapturedText(text);
        return CreateAsync(new CreateIdeaRequest(title, Category.InboxId, summary), ct);
    }

    public async Task<IdeaDetailDto> UpdateAsync(Guid id, UpdateIdeaRequest request, CancellationToken ct = default)
    {
        Validate(request.Title, request.Summary, request.Status, request.Excitement, request.Tags);

        var idea = await FindAsync(id, ct);
        if (idea.CategoryId != request.CategoryId)
        {
            // Moving between categories keeps the existing sections untouched.
            idea.Category = await FindCategoryAsync(request.CategoryId, ct);
            idea.CategoryId = request.CategoryId;
        }

        idea.Title = request.Title.Trim();
        idea.Summary = request.Summary?.Trim() ?? string.Empty;
        idea.Status = request.Status;
        idea.Excitement = request.Excitement;
        idea.Tags = NormalizeTags(request.Tags);
        idea.IsPinned = request.IsPinned;
        idea.UpdatedAt = clock.GetUtcNow().UtcDateTime;

        await ideas.SaveChangesAsync(ct);
        return idea.ToDetailDto();
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var idea = await FindAsync(id, ct);
        ideas.Remove(idea);
        await ideas.SaveChangesAsync(ct);
    }

    /// <summary>First line becomes the title (truncated if needed); the rest becomes the summary.</summary>
    internal static (string Title, string? Summary) SplitCapturedText(string text)
    {
        var lines = text.Split('\n', 2, StringSplitOptions.TrimEntries);
        var title = lines[0];
        var rest = lines.Length > 1 ? lines[1] : null;

        if (title.Length > MaxTitleLength)
        {
            rest = string.Join('\n', new[] { title, rest }.Where(s => !string.IsNullOrEmpty(s)));
            title = string.Concat(title.AsSpan(0, MaxTitleLength - 1), "…");
        }

        if (rest is { Length: > MaxSummaryLength })
        {
            rest = rest[..MaxSummaryLength];
        }

        return (title, string.IsNullOrWhiteSpace(rest) ? null : rest);
    }

    private async Task<Idea> FindAsync(Guid id, CancellationToken ct) =>
        await ideas.GetByIdAsync(id, ct) ?? throw new NotFoundException(nameof(Idea), id);

    private async Task<Category> FindCategoryAsync(Guid id, CancellationToken ct) =>
        await categories.GetByIdAsync(id, ct)
        ?? throw new ValidationException(new Dictionary<string, string[]>
        {
            ["CategoryId"] = [$"Category '{id}' does not exist."],
        });

    private static string NormalizeTag(string tag) => tag.Trim().TrimStart('#').ToLowerInvariant();

    private static List<string> NormalizeTags(IReadOnlyList<string>? tags) =>
        [.. (tags ?? []).Select(NormalizeTag).Where(t => t.Length > 0).Distinct()];

    private static void Validate(
        string? title, string? summary, IdeaStatus status, int excitement, IReadOnlyList<string>? tags)
    {
        var normalizedTags = NormalizeTags(tags);

        new ValidationErrors()
            .Required(title, "Title")
            .MaxLength(title?.Trim(), MaxTitleLength, "Title")
            .MaxLength(summary?.Trim(), MaxSummaryLength, "Summary")
            .When(!Enum.IsDefined(status), "Status", "Unknown status.")
            .When(excitement is < Idea.MinExcitement or > Idea.MaxExcitement,
                "Excitement", $"Must be between {Idea.MinExcitement} and {Idea.MaxExcitement}.")
            .When(normalizedTags.Count > MaxTags, "Tags", $"Must have at most {MaxTags} tags.")
            .When(normalizedTags.Any(t => t.Length > MaxTagLength),
                "Tags", $"Each tag must have at most {MaxTagLength} characters.")
            .ThrowIfAny();
    }
}
