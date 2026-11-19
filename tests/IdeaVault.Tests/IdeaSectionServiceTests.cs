using IdeaVault.Core.Dtos;
using IdeaVault.Core.Exceptions;
using IdeaVault.Tests.Support;

namespace IdeaVault.Tests;

public sealed class IdeaSectionServiceTests : IAsyncLifetime
{
    private readonly TestDatabase _db = new();
    private Guid _ideaId;

    public async Task InitializeAsync()
    {
        await _db.InitializeAsync();
        using var s = _db.Services();
        _ideaId = (await s.Ideas.CreateAsync(new CreateIdeaRequest("Jogo", Seeded.Games))).Id;
    }

    public Task DisposeAsync() => _db.DisposeAsync();

    [Fact]
    public async Task Add_appends_section_at_the_end()
    {
        using (var s = _db.Services())
        {
            await s.Sections.AddAsync(_ideaId, new SaveSectionRequest(" Monetização ", "F2P?"));
        }

        using var read = _db.Services();
        var sections = await read.Sections.GetAllAsync(_ideaId);

        Assert.Equal(6, sections.Count);
        Assert.Equal("Monetização", sections[^1].Title);
        Assert.Equal(5, sections[^1].SortOrder);
    }

    [Fact]
    public async Task Update_changes_title_and_markdown_content()
    {
        using var s = _db.Services();
        var first = (await s.Sections.GetAllAsync(_ideaId))[0];

        await s.Sections.UpdateAsync(_ideaId, first.Id, new SaveSectionRequest("Loop", "**Coletar → craftar**"));

        using var read = _db.Services();
        Assert.Equal("**Coletar → craftar**", (await read.Sections.GetAllAsync(_ideaId))[0].Content);
    }

    [Fact]
    public async Task Reorder_applies_the_given_order()
    {
        using var s = _db.Services();
        var ids = (await s.Sections.GetAllAsync(_ideaId)).Select(x => x.Id).Reverse().ToList();

        await s.Sections.ReorderAsync(_ideaId, new ReorderSectionsRequest(ids));

        using var read = _db.Services();
        var sections = await read.Sections.GetAllAsync(_ideaId);
        Assert.Equal(ids, sections.Select(x => x.Id));
        Assert.Equal("Próximo passo", sections[0].Title);
    }

    [Fact]
    public async Task Reorder_rejects_lists_that_are_not_a_permutation_of_the_sections()
    {
        using var s = _db.Services();
        var ids = (await s.Sections.GetAllAsync(_ideaId)).Select(x => x.Id).ToList();

        await Assert.ThrowsAsync<ValidationException>(() =>
            s.Sections.ReorderAsync(_ideaId, new ReorderSectionsRequest(ids[1..])));
        await Assert.ThrowsAsync<ValidationException>(() =>
            s.Sections.ReorderAsync(_ideaId, new ReorderSectionsRequest([.. ids[1..], ids[1]])));
    }

    [Fact]
    public async Task Delete_removes_only_that_section()
    {
        using var s = _db.Services();
        var first = (await s.Sections.GetAllAsync(_ideaId))[0];

        await s.Sections.DeleteAsync(_ideaId, first.Id);

        using var read = _db.Services();
        var remaining = await read.Sections.GetAllAsync(_ideaId);
        Assert.Equal(4, remaining.Count);
        Assert.DoesNotContain(remaining, x => x.Id == first.Id);
    }

    [Fact]
    public async Task Operations_on_a_section_of_another_idea_throw_not_found()
    {
        using var s = _db.Services();

        await Assert.ThrowsAsync<NotFoundException>(() =>
            s.Sections.DeleteAsync(_ideaId, Guid.NewGuid()));
    }
}
