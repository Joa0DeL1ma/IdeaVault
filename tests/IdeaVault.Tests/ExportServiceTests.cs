using IdeaVault.Core.Dtos;
using IdeaVault.Tests.Support;

namespace IdeaVault.Tests;

public sealed class ExportServiceTests : IAsyncLifetime
{
    private readonly TestDatabase _db = new();

    public Task InitializeAsync() => _db.InitializeAsync();
    public Task DisposeAsync() => _db.DisposeAsync();

    [Fact]
    public async Task Export_contains_every_category_idea_and_section()
    {
        using var s = _db.Services();
        await s.Ideas.CreateAsync(new CreateIdeaRequest("Jogo", Seeded.Games));
        _db.Clock.Advance(TimeSpan.FromMinutes(1));
        await s.Ideas.QuickCaptureAsync(new QuickCaptureRequest("Solta"));

        var export = await s.Export.ExportAsync();

        Assert.Equal(5, export.Categories.Count);
        Assert.Equal(["Jogo", "Solta"], export.Ideas.Select(i => i.Title));
        Assert.Equal(5, export.Ideas[0].Sections.Count);
        Assert.Equal(_db.Clock.GetUtcNow().UtcDateTime, export.ExportedAt);
    }
}
