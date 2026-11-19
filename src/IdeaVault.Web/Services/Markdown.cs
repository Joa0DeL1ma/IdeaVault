using Markdig;
using Microsoft.AspNetCore.Components;

namespace IdeaVault.Web.Services;

public static class Markdown
{
    // Raw HTML is disabled so section content can never inject scripts.
    private static readonly MarkdownPipeline Pipeline = new MarkdownPipelineBuilder()
        .UseAdvancedExtensions()
        .DisableHtml()
        .Build();

    public static MarkupString ToHtml(string? markdown) =>
        new(Markdig.Markdown.ToHtml(markdown ?? string.Empty, Pipeline));
}
