using IdeaVault.Core.Dtos;
using IdeaVault.Core.Entities;
using IdeaVault.Tests.Support;

namespace IdeaVault.Tests;

public sealed class IdeaSearchTests : IAsyncLifetime
{
    private readonly TestDatabase _db = new();

    public async Task InitializeAsync()
    {
        await _db.InitializeAsync();
        using var s = _db.Services();

        await Create(s, "Roguelike de cartas", Seeded.Games, excitement: 5, tags: ["indie", "cartas"]);
        await Create(s, "SaaS de agendamento", Seeded.Business, excitement: 2, status: IdeaStatus.Growing);
        await Create(s, "Plataforma 2D", Seeded.Games, excitement: 3, tags: ["indie"], pinned: true);
        var withContent = await Create(s, "Anotação solta", Seeded.Inbox, excitement: 1);
        await s.Sections.AddAsync(withContent.Id, new SaveSectionRequest("Notas", "Usar um dragão como chefe final"));
    }

    public Task DisposeAsync() => _db.DisposeAsync();

    private async Task<IdeaDetailDto> Create(
        TestDatabase.ServiceSet s, string title, Guid category, int excitement,
        IdeaStatus status = IdeaStatus.Seed, string[]? tags = null, bool pinned = false)
    {
        _db.Clock.Advance(TimeSpan.FromMinutes(1)); // later ideas are "more recently updated"
        return await s.Ideas.CreateAsync(new CreateIdeaRequest(title, category, null, status, excitement, tags, pinned));
    }

    private async Task<string[]> Titles(IdeaQuery query)
    {
        using var s = _db.Services();
        return [.. (await s.Ideas.SearchAsync(query)).Select(i => i.Title)];
    }

    [Fact]
    public async Task Without_filters_returns_pinned_first_then_most_recently_updated() =>
        Assert.Equal(
            ["Plataforma 2D", "Anotação solta", "SaaS de agendamento", "Roguelike de cartas"],
            await Titles(new IdeaQuery()));

    [Fact]
    public async Task Sort_by_excitement_keeps_pinned_first() =>
        Assert.Equal(
            ["Plataforma 2D", "Roguelike de cartas", "SaaS de agendamento", "Anotação solta"],
            await Titles(new IdeaQuery(Sort: IdeaSort.Excitement)));

    [Fact]
    public async Task Filters_by_category() =>
        Assert.Equal(["Plataforma 2D", "Roguelike de cartas"], await Titles(new IdeaQuery(CategoryId: Seeded.Games)));

    [Fact]
    public async Task Filters_by_status() =>
        Assert.Equal(["SaaS de agendamento"], await Titles(new IdeaQuery(Status: IdeaStatus.Growing)));

    [Fact]
    public async Task Filters_by_tag_ignoring_case_and_hash() =>
        Assert.Equal(["Plataforma 2D", "Roguelike de cartas"], await Titles(new IdeaQuery(Tag: "#INDIE")));

    [Theory]
    [InlineData("rogue", "Roguelike de cartas")]  // title
    [InlineData("CHEFE FINAL", "Anotação solta")] // section content, case-insensitive
    public async Task Search_matches_title_summary_and_section_content(string term, string expected) =>
        Assert.Equal([expected], await Titles(new IdeaQuery(Search: term)));

    [Fact]
    public async Task Combines_filters() =>
        Assert.Equal(["Roguelike de cartas"], await Titles(new IdeaQuery(Seeded.Games, Tag: "cartas", Search: "rogue")));
}
