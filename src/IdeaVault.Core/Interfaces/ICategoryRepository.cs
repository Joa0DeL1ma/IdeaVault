using IdeaVault.Core.Entities;

namespace IdeaVault.Core.Interfaces;

public interface ICategoryRepository
{
    Task<List<Category>> GetAllAsync(CancellationToken ct = default);
    Task<Dictionary<Guid, int>> GetIdeaCountsAsync(CancellationToken ct = default);
    Task<Category?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<bool> HasIdeasAsync(Guid id, CancellationToken ct = default);
    void Add(Category category);
    void Remove(Category category);
    Task SaveChangesAsync(CancellationToken ct = default);
}
