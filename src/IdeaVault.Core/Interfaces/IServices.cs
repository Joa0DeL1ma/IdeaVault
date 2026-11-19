using IdeaVault.Core.Dtos;

namespace IdeaVault.Core.Interfaces;

public interface ICategoryService
{
    Task<IReadOnlyList<CategoryDto>> GetAllAsync(CancellationToken ct = default);
    Task<CategoryDto> GetAsync(Guid id, CancellationToken ct = default);
    Task<CategoryDto> CreateAsync(SaveCategoryRequest request, CancellationToken ct = default);
    Task<CategoryDto> UpdateAsync(Guid id, SaveCategoryRequest request, CancellationToken ct = default);
    Task DeleteAsync(Guid id, CancellationToken ct = default);
}

public interface IIdeaService
{
    Task<IReadOnlyList<IdeaSummaryDto>> SearchAsync(IdeaQuery query, CancellationToken ct = default);
    Task<IdeaDetailDto> GetAsync(Guid id, CancellationToken ct = default);
    Task<IdeaDetailDto> CreateAsync(CreateIdeaRequest request, CancellationToken ct = default);
    Task<IdeaDetailDto> QuickCaptureAsync(QuickCaptureRequest request, CancellationToken ct = default);
    Task<IdeaDetailDto> UpdateAsync(Guid id, UpdateIdeaRequest request, CancellationToken ct = default);
    Task DeleteAsync(Guid id, CancellationToken ct = default);
}

public interface IIdeaSectionService
{
    Task<IReadOnlyList<IdeaSectionDto>> GetAllAsync(Guid ideaId, CancellationToken ct = default);
    Task<IdeaSectionDto> AddAsync(Guid ideaId, SaveSectionRequest request, CancellationToken ct = default);
    Task<IdeaSectionDto> UpdateAsync(Guid ideaId, Guid sectionId, SaveSectionRequest request, CancellationToken ct = default);
    Task DeleteAsync(Guid ideaId, Guid sectionId, CancellationToken ct = default);
    Task<IReadOnlyList<IdeaSectionDto>> ReorderAsync(Guid ideaId, ReorderSectionsRequest request, CancellationToken ct = default);
}

public interface IExportService
{
    Task<ExportDto> ExportAsync(CancellationToken ct = default);
}
