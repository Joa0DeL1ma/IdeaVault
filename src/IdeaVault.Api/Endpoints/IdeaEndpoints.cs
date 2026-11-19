using IdeaVault.Core.Dtos;
using IdeaVault.Core.Interfaces;
using Microsoft.AspNetCore.Http.HttpResults;

namespace IdeaVault.Api.Endpoints;

public static class IdeaEndpoints
{
    public static RouteGroupBuilder MapIdeaEndpoints(this RouteGroupBuilder api)
    {
        var group = api.MapGroup("/ideas").WithTags("Ideas");

        group.MapGet("/", async ([AsParameters] IdeaQuery query, IIdeaService service, CancellationToken ct) =>
            TypedResults.Ok(await service.SearchAsync(query, ct)))
            .WithSummary("Lists ideas (pinned first) filtered by category, status, tag and free-text search.");

        group.MapGet("/{id:guid}", async (Guid id, IIdeaService service, CancellationToken ct) =>
            TypedResults.Ok(await service.GetAsync(id, ct)));

        group.MapPost("/", async Task<Created<IdeaDetailDto>> (
            CreateIdeaRequest request, IIdeaService service, CancellationToken ct) =>
            Created(await service.CreateAsync(request, ct)))
            .WithSummary("Creates an idea; the category template becomes empty sections.");

        group.MapPost("/quick", async Task<Created<IdeaDetailDto>> (
            QuickCaptureRequest request, IIdeaService service, CancellationToken ct) =>
            Created(await service.QuickCaptureAsync(request, ct)))
            .WithSummary("Quick capture: free text that lands in the inbox.");

        group.MapPut("/{id:guid}", async (
            Guid id, UpdateIdeaRequest request, IIdeaService service, CancellationToken ct) =>
            TypedResults.Ok(await service.UpdateAsync(id, request, ct)));

        group.MapDelete("/{id:guid}", async (Guid id, IIdeaService service, CancellationToken ct) =>
        {
            await service.DeleteAsync(id, ct);
            return TypedResults.NoContent();
        });

        return api;
    }

    private static Created<IdeaDetailDto> Created(IdeaDetailDto idea) =>
        TypedResults.Created($"/api/ideas/{idea.Id}", idea);
}
