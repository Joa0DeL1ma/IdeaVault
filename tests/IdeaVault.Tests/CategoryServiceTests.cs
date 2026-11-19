using IdeaVault.Core.Dtos;
using IdeaVault.Core.Exceptions;
using IdeaVault.Tests.Support;

namespace IdeaVault.Tests;

public sealed class CategoryServiceTests : IAsyncLifetime
{
    private readonly TestDatabase _db = new();

    public Task InitializeAsync() => _db.InitializeAsync();
    public Task DisposeAsync() => _db.DisposeAsync();

    [Fact]
    public async Task Seed_creates_the_five_default_categories_in_order()
    {
        using var s = _db.Services();

        var categories = await s.Categories.GetAllAsync();

        Assert.Equal(["Caixa de entrada", "Jogos", "Negócios", "Histórias", "Tecnologia/Pesquisa"],
            categories.Select(c => c.Name));
        Assert.True(categories[0].IsInbox);
        Assert.Empty(categories[0].TemplateSections);
    }

    [Fact]
    public async Task Create_trims_and_deduplicates_template_sections()
    {
        using var s = _db.Services();

        var category = await s.Categories.CreateAsync(
            new SaveCategoryRequest("Música", "🎵", "#ff00aa", 5, [" Letra ", "letra", "", "Arranjo"]));

        Assert.Equal(["Letra", "Arranjo"], category.TemplateSections);
        Assert.Equal("#FF00AA", category.Color);
    }

    [Theory]
    [InlineData("red")]
    [InlineData("#12345")]
    [InlineData("#GGGGGG")]
    public async Task Create_rejects_invalid_colors(string color)
    {
        using var s = _db.Services();

        var ex = await Assert.ThrowsAsync<ValidationException>(() =>
            s.Categories.CreateAsync(new SaveCategoryRequest("Música", "🎵", color)));

        Assert.Contains("Color", ex.Errors.Keys);
    }

    [Fact]
    public async Task Inbox_cannot_be_deleted()
    {
        using var s = _db.Services();

        await Assert.ThrowsAsync<ConflictException>(() => s.Categories.DeleteAsync(Seeded.Inbox));
    }

    [Fact]
    public async Task Category_with_ideas_cannot_be_deleted()
    {
        using var s = _db.Services();
        await s.Ideas.CreateAsync(new CreateIdeaRequest("Jogo", Seeded.Games));

        await Assert.ThrowsAsync<ConflictException>(() => s.Categories.DeleteAsync(Seeded.Games));
    }

    [Fact]
    public async Task Empty_category_can_be_deleted()
    {
        using (var s = _db.Services())
        {
            await s.Categories.DeleteAsync(Seeded.Business);
        }

        using var read = _db.Services();
        await Assert.ThrowsAsync<NotFoundException>(() => read.Categories.GetAsync(Seeded.Business));
    }

    [Fact]
    public async Task Get_all_includes_idea_counts()
    {
        using var s = _db.Services();
        await s.Ideas.QuickCaptureAsync(new QuickCaptureRequest("a"));
        await s.Ideas.QuickCaptureAsync(new QuickCaptureRequest("b"));

        var inbox = (await s.Categories.GetAllAsync()).Single(c => c.IsInbox);

        Assert.Equal(2, inbox.IdeaCount);
    }
}
