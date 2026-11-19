using IdeaVault.Core.Dtos;
using IdeaVault.Core.Entities;
using IdeaVault.Core.Exceptions;
using IdeaVault.Core.Interfaces;
using IdeaVault.Core.Mapping;
using IdeaVault.Core.Validation;

namespace IdeaVault.Core.Services;

public sealed class IdeaSectionService(IIdeaRepository ideas, TimeProvider clock) : IIdeaSectionService
{
    public const int MaxTitleLength = 100;
    public const int MaxContentLength = 50_000;

    public async Task<IReadOnlyList<IdeaSectionDto>> GetAllAsync(Guid ideaId, CancellationToken ct = default) =>
        Ordered(await FindIdeaAsync(ideaId, ct));

    public async Task<IdeaSectionDto> AddAsync(Guid ideaId, SaveSectionRequest request, CancellationToken ct = default)
    {
        Validate(request);
        var idea = await FindIdeaAsync(ideaId, ct);

        var section = new IdeaSection
        {
            Title = request.Title.Trim(),
            Content = request.Content ?? string.Empty,
            SortOrder = idea.Sections.Count == 0 ? 0 : idea.Sections.Max(s => s.SortOrder) + 1,
        };

        idea.Sections.Add(section);
        await TouchAndSaveAsync(idea, ct);

        return section.ToDto();
    }

    public async Task<IdeaSectionDto> UpdateAsync(
        Guid ideaId, Guid sectionId, SaveSectionRequest request, CancellationToken ct = default)
    {
        Validate(request);
        var idea = await FindIdeaAsync(ideaId, ct);
        var section = FindSection(idea, sectionId);

        section.Title = request.Title.Trim();
        section.Content = request.Content ?? string.Empty;
        await TouchAndSaveAsync(idea, ct);

        return section.ToDto();
    }

    public async Task DeleteAsync(Guid ideaId, Guid sectionId, CancellationToken ct = default)
    {
        var idea = await FindIdeaAsync(ideaId, ct);
        idea.Sections.Remove(FindSection(idea, sectionId));
        await TouchAndSaveAsync(idea, ct);
    }

    public async Task<IReadOnlyList<IdeaSectionDto>> ReorderAsync(
        Guid ideaId, ReorderSectionsRequest request, CancellationToken ct = default)
    {
        var idea = await FindIdeaAsync(ideaId, ct);
        var ids = request.SectionIds ?? [];

        var isPermutation = ids.Count == idea.Sections.Count
            && ids.Distinct().Count() == ids.Count
            && ids.All(id => idea.Sections.Any(s => s.Id == id));

        new ValidationErrors()
            .When(!isPermutation, nameof(request.SectionIds), "Must contain every section id of the idea exactly once.")
            .ThrowIfAny();

        for (var index = 0; index < ids.Count; index++)
        {
            FindSection(idea, ids[index]).SortOrder = index;
        }

        await TouchAndSaveAsync(idea, ct);
        return Ordered(idea);
    }

    private async Task<Idea> FindIdeaAsync(Guid ideaId, CancellationToken ct) =>
        await ideas.GetByIdAsync(ideaId, ct) ?? throw new NotFoundException(nameof(Idea), ideaId);

    private static IdeaSection FindSection(Idea idea, Guid sectionId) =>
        idea.Sections.FirstOrDefault(s => s.Id == sectionId)
        ?? throw new NotFoundException(nameof(IdeaSection), sectionId);

    private static List<IdeaSectionDto> Ordered(Idea idea) =>
        [.. idea.Sections.OrderBy(s => s.SortOrder).Select(s => s.ToDto())];

    private Task TouchAndSaveAsync(Idea idea, CancellationToken ct)
    {
        idea.UpdatedAt = clock.GetUtcNow().UtcDateTime;
        return ideas.SaveChangesAsync(ct);
    }

    private static void Validate(SaveSectionRequest request) =>
        new ValidationErrors()
            .Required(request.Title, nameof(request.Title))
            .MaxLength(request.Title?.Trim(), MaxTitleLength, nameof(request.Title))
            .MaxLength(request.Content, MaxContentLength, nameof(request.Content))
            .ThrowIfAny();
}
