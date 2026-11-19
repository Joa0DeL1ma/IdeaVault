using Microsoft.EntityFrameworkCore;

namespace IdeaVault.Infra.Data;

public static class DatabaseInitializer
{
    /// <summary>Applies pending migrations and inserts the default categories on an empty database.</summary>
    public static async Task InitializeAsync(IdeaVaultDbContext db, CancellationToken cancellationToken = default)
    {
        await db.Database.MigrateAsync(cancellationToken);
        await SeedAsync(db, cancellationToken);
    }

    /// <summary>Inserts the default categories when none exist (idempotent).</summary>
    public static async Task SeedAsync(IdeaVaultDbContext db, CancellationToken cancellationToken = default)
    {
        if (await db.Categories.AnyAsync(cancellationToken))
        {
            return;
        }

        db.Categories.AddRange(SeedData.CreateCategories());
        await db.SaveChangesAsync(cancellationToken);
    }
}
