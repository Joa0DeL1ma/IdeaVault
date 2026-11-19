using IdeaVault.Core.Entities;
using IdeaVault.Core.Interfaces;
using IdeaVault.Infra.Data;
using Microsoft.EntityFrameworkCore;

namespace IdeaVault.Infra.Repositories;

internal sealed class CategoryRepository(IdeaVaultDbContext db) : ICategoryRepository
{
    public Task<List<Category>> GetAllAsync(CancellationToken ct = default) =>
        db.Categories.AsNoTracking()
            .OrderBy(c => c.SortOrder).ThenBy(c => c.Name)
            .ToListAsync(ct);

    public Task<Dictionary<Guid, int>> GetIdeaCountsAsync(CancellationToken ct = default) =>
        db.Ideas.GroupBy(i => i.CategoryId)
            .Select(g => new { CategoryId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.CategoryId, x => x.Count, ct);

    public Task<Category?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        db.Categories.FirstOrDefaultAsync(c => c.Id == id, ct);

    public Task<bool> HasIdeasAsync(Guid id, CancellationToken ct = default) =>
        db.Ideas.AnyAsync(i => i.CategoryId == id, ct);

    public void Add(Category category) => db.Categories.Add(category);

    public void Remove(Category category) => db.Categories.Remove(category);

    public Task SaveChangesAsync(CancellationToken ct = default) => db.SaveChangesAsync(ct);
}
