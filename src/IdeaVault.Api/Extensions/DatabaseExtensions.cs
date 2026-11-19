using IdeaVault.Infra.Data;

namespace IdeaVault.Api.Extensions;

public static class DatabaseExtensions
{
    /// <summary>Applies pending EF Core migrations and seeds default data at startup.</summary>
    public static async Task InitializeDatabaseAsync(this WebApplication app)
    {
        await using var scope = app.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<IdeaVaultDbContext>();
        await DatabaseInitializer.InitializeAsync(db, app.Lifetime.ApplicationStopping);
    }
}
