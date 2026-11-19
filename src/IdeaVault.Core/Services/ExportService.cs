using IdeaVault.Core.Dtos;
using IdeaVault.Core.Interfaces;
using IdeaVault.Core.Mapping;

namespace IdeaVault.Core.Services;

public sealed class ExportService(
    ICategoryService categoryService,
    IIdeaRepository ideas,
    TimeProvider clock) : IExportService
{
    public const int SchemaVersion = 1;

    public async Task<ExportDto> ExportAsync(CancellationToken ct = default)
    {
        var categories = await categoryService.GetAllAsync(ct);
        var allIdeas = await ideas.GetAllWithSectionsAsync(ct);

        return new ExportDto(
            SchemaVersion,
            clock.GetUtcNow().UtcDateTime,
            categories,
            [.. allIdeas.Select(i => i.ToDetailDto())]);
    }
}
