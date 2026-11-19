using IdeaVault.Core.Dtos;
using IdeaVault.Core.Entities;
using IdeaVault.Core.Exceptions;
using IdeaVault.Core.Services;
using IdeaVault.Tests.Support;

namespace IdeaVault.Tests;

public sealed class IdeaServiceTests : IAsyncLifetime
{
    private readonly TestDatabase _db = new();

    public Task InitializeAsync() => _db.InitializeAsync();
    public Task DisposeAsync() => _db.DisposeAsync();

    [Fact]
    public async Task Create_in_category_with_template_creates_empty_sections_in_template_order()
    {
        using var s = _db.Services();

        var idea = await s.Ideas.CreateAsync(new CreateIdeaRequest("Roguelike de cartas", Seeded.Games));

        Assert.Equal(
            ["Mecânica central", "Gênero", "Referências", "Escopo/MVP", "Próximo passo"],
            idea.Sections.Select(x => x.Title));
        Assert.All(idea.Sections, x => Assert.Empty(x.Content));
        Assert.Equal([0, 1, 2, 3, 4], idea.Sections.Select(x => x.SortOrder));
    }

    [Fact]
    public async Task Create_persists_sections_so_they_can_be_read_back()
    {
        Guid id;
        using (var s = _db.Services())
        {
            id = (await s.Ideas.CreateAsync(new CreateIdeaRequest("App de finanças", Seeded.Business))).Id;
        }

        using var read = _db.Services();
        var idea = await read.Ideas.GetAsync(id);

        Assert.Equal(5, idea.Sections.Count);
        Assert.Equal("Problema", idea.Sections[0].Title);
    }

    [Fact]
    public async Task Create_in_inbox_creates_no_sections()
    {
        using var s = _db.Services();

        var idea = await s.Ideas.CreateAsync(new CreateIdeaRequest("Algo solto", Seeded.Inbox));

        Assert.Empty(idea.Sections);
    }

    [Fact]
    public async Task Create_normalizes_tags()
    {
        using var s = _db.Services();

        var idea = await s.Ideas.CreateAsync(new CreateIdeaRequest(
            "Ideia", Seeded.Inbox, Tags: [" #Indie ", "indie", "", "Coop"]));

        Assert.Equal(["indie", "coop"], idea.Tags);
    }

    [Fact]
    public async Task Create_with_unknown_category_fails_validation()
    {
        using var s = _db.Services();

        var ex = await Assert.ThrowsAsync<ValidationException>(() =>
            s.Ideas.CreateAsync(new CreateIdeaRequest("Ideia", Guid.NewGuid())));

        Assert.Contains("CategoryId", ex.Errors.Keys);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(6)]
    [InlineData(-3)]
    public async Task Create_rejects_excitement_outside_1_to_5(int excitement)
    {
        using var s = _db.Services();

        var ex = await Assert.ThrowsAsync<ValidationException>(() =>
            s.Ideas.CreateAsync(new CreateIdeaRequest("Ideia", Seeded.Inbox, Excitement: excitement)));

        Assert.Contains("Excitement", ex.Errors.Keys);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(5)]
    public async Task Create_accepts_excitement_at_the_boundaries(int excitement)
    {
        using var s = _db.Services();

        var idea = await s.Ideas.CreateAsync(new CreateIdeaRequest("Ideia", Seeded.Inbox, Excitement: excitement));

        Assert.Equal(excitement, idea.Excitement);
    }

    [Fact]
    public async Task Create_reports_all_invalid_fields_at_once()
    {
        using var s = _db.Services();

        var ex = await Assert.ThrowsAsync<ValidationException>(() =>
            s.Ideas.CreateAsync(new CreateIdeaRequest("  ", Seeded.Inbox, Excitement: 9)));

        Assert.Contains("Title", ex.Errors.Keys);
        Assert.Contains("Excitement", ex.Errors.Keys);
    }

    [Fact]
    public async Task Quick_capture_goes_to_inbox_as_a_seed()
    {
        using var s = _db.Services();

        var idea = await s.Ideas.QuickCaptureAsync(new QuickCaptureRequest("  Jogo de pescaria no espaço  "));

        Assert.Equal(Seeded.Inbox, idea.CategoryId);
        Assert.Equal("Jogo de pescaria no espaço", idea.Title);
        Assert.Equal(IdeaStatus.Seed, idea.Status);
        Assert.Empty(idea.Sections);
    }

    [Fact]
    public async Task Quick_capture_uses_first_line_as_title_and_the_rest_as_summary()
    {
        using var s = _db.Services();

        var idea = await s.Ideas.QuickCaptureAsync(new QuickCaptureRequest("Título\nlinha 2\nlinha 3"));

        Assert.Equal("Título", idea.Title);
        Assert.Equal("linha 2\nlinha 3", idea.Summary);
    }

    [Fact]
    public void Quick_capture_truncates_long_titles_and_keeps_full_text_in_summary()
    {
        var text = new string('a', 250);

        var (title, summary) = IdeaService.SplitCapturedText(text);

        Assert.Equal(IdeaService.MaxTitleLength, title.Length);
        Assert.EndsWith("…", title);
        Assert.Equal(text, summary);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task Quick_capture_rejects_empty_text(string text)
    {
        using var s = _db.Services();

        await Assert.ThrowsAsync<ValidationException>(() => s.Ideas.QuickCaptureAsync(new QuickCaptureRequest(text)));
    }

    [Fact]
    public async Task Update_changes_fields_and_bumps_updated_at()
    {
        using var s = _db.Services();
        var created = await s.Ideas.CreateAsync(new CreateIdeaRequest("Antes", Seeded.Inbox));
        _db.Clock.Advance(TimeSpan.FromHours(1));

        var updated = await s.Ideas.UpdateAsync(created.Id, new UpdateIdeaRequest(
            "Depois", Seeded.Games, "Resumo", IdeaStatus.Growing, 5, ["rpg"], IsPinned: true));

        Assert.Equal("Depois", updated.Title);
        Assert.Equal(Seeded.Games, updated.CategoryId);
        Assert.Equal(IdeaStatus.Growing, updated.Status);
        Assert.True(updated.IsPinned);
        Assert.Equal(created.CreatedAt, updated.CreatedAt);
        Assert.Equal(created.UpdatedAt.AddHours(1), updated.UpdatedAt);
        Assert.Empty(updated.Sections); // changing category does not re-apply the template
    }

    [Fact]
    public async Task Update_rejects_invalid_excitement()
    {
        using var s = _db.Services();
        var created = await s.Ideas.CreateAsync(new CreateIdeaRequest("Ideia", Seeded.Inbox));

        await Assert.ThrowsAsync<ValidationException>(() => s.Ideas.UpdateAsync(created.Id,
            new UpdateIdeaRequest("Ideia", Seeded.Inbox, null, IdeaStatus.Seed, 0, null, false)));
    }

    [Fact]
    public async Task Get_unknown_idea_throws_not_found()
    {
        using var s = _db.Services();

        await Assert.ThrowsAsync<NotFoundException>(() => s.Ideas.GetAsync(Guid.NewGuid()));
    }

    [Fact]
    public async Task Delete_removes_idea_and_its_sections()
    {
        Guid id;
        using (var s = _db.Services())
        {
            id = (await s.Ideas.CreateAsync(new CreateIdeaRequest("Ideia", Seeded.Games))).Id;
            await s.Ideas.DeleteAsync(id);
        }

        using var read = _db.Services();
        await Assert.ThrowsAsync<NotFoundException>(() => read.Ideas.GetAsync(id));
        await using var ctx = _db.CreateContext();
        Assert.Empty(ctx.IdeaSections);
    }
}
