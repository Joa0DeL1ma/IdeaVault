using IdeaVault.Core.Services;
using IdeaVault.Infra.Data;
using IdeaVault.Infra.Repositories;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Time.Testing;

namespace IdeaVault.Tests.Support;

/// <summary>
/// Seeded SQLite in-memory database. Each call to <see cref="Services"/> uses a fresh DbContext over the
/// same connection, mimicking separate HTTP requests (so change-tracking bugs are not hidden).
/// </summary>
public sealed class TestDatabase : IAsyncLifetime
{
    private readonly SqliteConnection _connection = new("DataSource=:memory:");

    public FakeTimeProvider Clock { get; } = new(new DateTimeOffset(2026, 1, 1, 12, 0, 0, TimeSpan.Zero));

    public async Task InitializeAsync()
    {
        await _connection.OpenAsync();
        await using var db = CreateContext();
        await db.Database.EnsureCreatedAsync();
        await DatabaseInitializer.SeedAsync(db);
    }

    public Task DisposeAsync() => _connection.DisposeAsync().AsTask();

    public IdeaVaultDbContext CreateContext() =>
        new(new DbContextOptionsBuilder<IdeaVaultDbContext>().UseSqlite(_connection).Options);

    public ServiceSet Services() => new(CreateContext(), Clock);

    public sealed class ServiceSet : IDisposable
    {
        private readonly IdeaVaultDbContext _db;

        public ServiceSet(IdeaVaultDbContext db, TimeProvider clock)
        {
            _db = db;
            var ideaRepository = new IdeaRepository(db);
            var categoryRepository = new CategoryRepository(db);

            Categories = new CategoryService(categoryRepository);
            Ideas = new IdeaService(ideaRepository, categoryRepository, clock);
            Sections = new IdeaSectionService(ideaRepository, clock);
            Export = new ExportService(Categories, ideaRepository, clock);
        }

        public CategoryService Categories { get; }
        public IdeaService Ideas { get; }
        public IdeaSectionService Sections { get; }
        public ExportService Export { get; }

        public void Dispose() => _db.Dispose();
    }
}
