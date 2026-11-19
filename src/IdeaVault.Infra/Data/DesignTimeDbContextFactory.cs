using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace IdeaVault.Infra.Data;

/// <summary>Used only by <c>dotnet ef</c> to generate migrations; no database connection is opened.</summary>
internal sealed class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<IdeaVaultDbContext>
{
    public IdeaVaultDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<IdeaVaultDbContext>()
            .UseNpgsql("Host=localhost;Database=ideavault;Username=postgres;Password=postgres")
            .Options;

        return new IdeaVaultDbContext(options);
    }
}
