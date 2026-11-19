using IdeaVault.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace IdeaVault.Infra.Data;

public class IdeaVaultDbContext(DbContextOptions<IdeaVaultDbContext> options) : DbContext(options)
{
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Idea> Ideas => Set<Idea>();
    public DbSet<IdeaSection> IdeaSections => Set<IdeaSection>();

    protected override void OnModelCreating(ModelBuilder modelBuilder) =>
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(IdeaVaultDbContext).Assembly);
}
