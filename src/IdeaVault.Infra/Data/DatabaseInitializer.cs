using Microsoft.EntityFrameworkCore;

namespace IdeaVault.Infra.Data;

public static class DatabaseInitializer
{
    /// <summary>Applies pending migrations and inserts the default categories on an empty database.</summary>
    public static async Task InitializeAsync(IdeaVaultDbContext db, CancellationToken cancellationToken = default)
    {
        await db.Database.MigrateAsync(cancellationToken);

        if (await db.Categories.AnyAsync(cancellationToken))
        {
            return;
        }

        db.Categories.AddRange(SeedData.CreateCategories());
        await db.SaveChangesAsync(cancellationToken);
    }
}
