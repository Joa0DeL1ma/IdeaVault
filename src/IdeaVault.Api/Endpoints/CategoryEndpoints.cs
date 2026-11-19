using IdeaVault.Core.Dtos;
using IdeaVault.Core.Interfaces;
using Microsoft.AspNetCore.Http.HttpResults;

namespace IdeaVault.Api.Endpoints;

public static class CategoryEndpoints
{
    public static RouteGroupBuilder MapCategoryEndpoints(this RouteGroupBuilder api)
    {
        var group = api.MapGroup("/categories").WithTags("Categories");

        group.MapGet("/", async (ICategoryService service, CancellationToken ct) =>
            TypedResults.Ok(await service.GetAllAsync(ct)));

        group.MapGet("/{id:guid}", async (Guid id, ICategoryService service, CancellationToken ct) =>
            TypedResults.Ok(await service.GetAsync(id, ct)));

        group.MapPost("/", async Task<Created<CategoryDto>> (
            SaveCategoryRequest request, ICategoryService service, CancellationToken ct) =>
        {
            var category = await service.CreateAsync(request, ct);
            return TypedResults.Created($"/api/categories/{category.Id}", category);
        });

        group.MapPut("/{id:guid}", async (
            Guid id, SaveCategoryRequest request, ICategoryService service, CancellationToken ct) =>
            TypedResults.Ok(await service.UpdateAsync(id, request, ct)));

        group.MapDelete("/{id:guid}", async (Guid id, ICategoryService service, CancellationToken ct) =>
        {
            await service.DeleteAsync(id, ct);
            return TypedResults.NoContent();
        });

        return api;
    }
}
