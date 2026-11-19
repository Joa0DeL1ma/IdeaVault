using System.Text.RegularExpressions;
using IdeaVault.Core.Dtos;
using IdeaVault.Core.Entities;
using IdeaVault.Core.Exceptions;
using IdeaVault.Core.Interfaces;
using IdeaVault.Core.Mapping;
using IdeaVault.Core.Validation;

namespace IdeaVault.Core.Services;

public sealed partial class CategoryService(ICategoryRepository categories) : ICategoryService
{
    public const int MaxTemplateSections = 20;

    public async Task<IReadOnlyList<CategoryDto>> GetAllAsync(CancellationToken ct = default)
    {
        var all = await categories.GetAllAsync(ct);
        var counts = await categories.GetIdeaCountsAsync(ct);
        return [.. all.Select(c => c.ToDto(counts.GetValueOrDefault(c.Id)))];
    }

    public async Task<CategoryDto> GetAsync(Guid id, CancellationToken ct = default)
    {
        var category = await FindAsync(id, ct);
        var counts = await categories.GetIdeaCountsAsync(ct);
        return category.ToDto(counts.GetValueOrDefault(id));
    }

    public async Task<CategoryDto> CreateAsync(SaveCategoryRequest request, CancellationToken ct = default)
    {
        Validate(request);

        var category = new Category { Name = "", Emoji = "", Color = "" };
        Apply(category, request);
        categories.Add(category);
        await categories.SaveChangesAsync(ct);

        return category.ToDto();
    }

    public async Task<CategoryDto> UpdateAsync(Guid id, SaveCategoryRequest request, CancellationToken ct = default)
    {
        Validate(request);

        var category = await FindAsync(id, ct);
        Apply(category, request);
        await categories.SaveChangesAsync(ct);

        return await GetAsync(id, ct);
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var category = await FindAsync(id, ct);

        if (category.IsInbox)
        {
            throw new ConflictException("The inbox category cannot be deleted.");
        }

        if (await categories.HasIdeasAsync(id, ct))
        {
            throw new ConflictException("Move or delete the ideas of this category before deleting it.");
        }

        categories.Remove(category);
        await categories.SaveChangesAsync(ct);
    }

    private async Task<Category> FindAsync(Guid id, CancellationToken ct) =>
        await categories.GetByIdAsync(id, ct) ?? throw new NotFoundException(nameof(Category), id);

    private static void Apply(Category category, SaveCategoryRequest request)
    {
        category.Name = request.Name.Trim();
        category.Emoji = request.Emoji.Trim();
        category.Color = request.Color.Trim().ToUpperInvariant();
        category.SortOrder = request.SortOrder;
        category.TemplateSections = [.. (request.TemplateSections ?? [])
            .Select(t => t.Trim())
            .Where(t => t.Length > 0)
            .Distinct(StringComparer.OrdinalIgnoreCase)];
    }

    private static void Validate(SaveCategoryRequest request)
    {
        var template = request.TemplateSections ?? [];

        new ValidationErrors()
            .Required(request.Name, nameof(request.Name))
            .MaxLength(request.Name?.Trim(), 50, nameof(request.Name))
            .Required(request.Emoji, nameof(request.Emoji))
            .MaxLength(request.Emoji?.Trim(), 16, nameof(request.Emoji))
            .When(request.Color is null || !HexColor().IsMatch(request.Color.Trim()),
                nameof(request.Color), "Must be a hex color like #7C5CFF.")
            .When(template.Count > MaxTemplateSections,
                nameof(request.TemplateSections), $"Must have at most {MaxTemplateSections} sections.")
            .When(template.Any(t => t?.Trim().Length > IdeaSectionService.MaxTitleLength),
                nameof(request.TemplateSections), $"Section titles must have at most {IdeaSectionService.MaxTitleLength} characters.")
            .ThrowIfAny();
    }

    [GeneratedRegex("^#[0-9A-Fa-f]{6}$")]
    private static partial Regex HexColor();
}
