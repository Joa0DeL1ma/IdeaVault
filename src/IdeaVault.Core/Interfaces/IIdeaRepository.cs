using IdeaVault.Core.Dtos;
using IdeaVault.Core.Entities;

namespace IdeaVault.Core.Interfaces;

public interface IIdeaRepository
{
    /// <summary>Returns ideas (with their category) matching the filters, pinned first.</summary>
    Task<List<Idea>> SearchAsync(IdeaQuery query, CancellationToken ct = default);

    /// <summary>Returns a tracked idea with its category and sections, or <c>null</c>.</summary>
    Task<Idea?> GetByIdAsync(Guid id, CancellationToken ct = default);

    Task<List<Idea>> GetAllWithSectionsAsync(CancellationToken ct = default);
    void Add(Idea idea);
    void Remove(Idea idea);
    Task SaveChangesAsync(CancellationToken ct = default);
}
