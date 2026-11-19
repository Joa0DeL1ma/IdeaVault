using IdeaVault.Core.Dtos;
using IdeaVault.Core.Interfaces;
using Microsoft.AspNetCore.Http.HttpResults;

namespace IdeaVault.Api.Endpoints;

public static class SectionEndpoints
{
    public static RouteGroupBuilder MapSectionEndpoints(this RouteGroupBuilder api)
    {
        var group = api.MapGroup("/ideas/{ideaId:guid}/sections").WithTags("Sections");

        group.MapGet("/", async (Guid ideaId, IIdeaSectionService service, CancellationToken ct) =>
            TypedResults.Ok(await service.GetAllAsync(ideaId, ct)));

        group.MapPost("/", async Task<Created<IdeaSectionDto>> (
            Guid ideaId, SaveSectionRequest request, IIdeaSectionService service, CancellationToken ct) =>
        {
            var section = await service.AddAsync(ideaId, request, ct);
            return TypedResults.Created($"/api/ideas/{ideaId}/sections/{section.Id}", section);
        });

        group.MapPut("/{sectionId:guid}", async (
            Guid ideaId, Guid sectionId, SaveSectionRequest request, IIdeaSectionService service, CancellationToken ct) =>
            TypedResults.Ok(await service.UpdateAsync(ideaId, sectionId, request, ct)));

        group.MapDelete("/{sectionId:guid}", async (
            Guid ideaId, Guid sectionId, IIdeaSectionService service, CancellationToken ct) =>
        {
            await service.DeleteAsync(ideaId, sectionId, ct);
            return TypedResults.NoContent();
        });

        group.MapPut("/order", async (
            Guid ideaId, ReorderSectionsRequest request, IIdeaSectionService service, CancellationToken ct) =>
            TypedResults.Ok(await service.ReorderAsync(ideaId, request, ct)))
            .WithSummary("Reorders sections; the body must list every section id of the idea.");

        return api;
    }
}
