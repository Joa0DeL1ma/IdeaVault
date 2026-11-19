using IdeaVault.Core.Dtos;
using IdeaVault.Core.Entities;
using IdeaVault.Core.Interfaces;
using IdeaVault.Infra.Data;
using Microsoft.EntityFrameworkCore;

namespace IdeaVault.Infra.Repositories;

internal sealed class IdeaRepository(IdeaVaultDbContext db) : IIdeaRepository
{
    public Task<List<Idea>> SearchAsync(IdeaQuery query, CancellationToken ct = default)
    {
        IQueryable<Idea> ideas = db.Ideas.AsNoTracking().Include(i => i.Category);

        if (query.CategoryId is { } categoryId)
        {
            ideas = ideas.Where(i => i.CategoryId == categoryId);
        }

        if (query.Status is { } status)
        {
            ideas = ideas.Where(i => i.Status == status);
        }

        if (query.Tag is { } tag)
        {
            ideas = ideas.Where(i => i.Tags.Contains(tag));
        }

        if (query.Search is { } search)
        {
            var term = search.ToLower();
            ideas = ideas.Where(i =>
                i.Title.ToLower().Contains(term)
                || i.Summary.ToLower().Contains(term)
                || i.Sections.Any(s => s.Content.ToLower().Contains(term)));
        }

        var pinnedFirst = ideas.OrderByDescending(i => i.IsPinned);
        var ordered = query.Sort == IdeaSort.Excitement
            ? pinnedFirst.ThenByDescending(i => i.Excitement).ThenByDescending(i => i.UpdatedAt)
            : pinnedFirst.ThenByDescending(i => i.UpdatedAt);

        return ordered.ToListAsync(ct);
    }

    public Task<Idea?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        db.Ideas
            .Include(i => i.Category)
            .Include(i => i.Sections)
            .FirstOrDefaultAsync(i => i.Id == id, ct);

    public Task<List<Idea>> GetAllWithSectionsAsync(CancellationToken ct = default) =>
        db.Ideas.AsNoTracking()
            .Include(i => i.Category)
            .Include(i => i.Sections)
            .OrderBy(i => i.CreatedAt)
            .AsSplitQuery()
            .ToListAsync(ct);

    public void Add(Idea idea) => db.Ideas.Add(idea);

    public void Remove(Idea idea) => db.Ideas.Remove(idea);

    public Task SaveChangesAsync(CancellationToken ct = default) => db.SaveChangesAsync(ct);
}
