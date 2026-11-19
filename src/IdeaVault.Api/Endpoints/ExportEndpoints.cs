using IdeaVault.Core.Interfaces;

namespace IdeaVault.Api.Endpoints;

public static class ExportEndpoints
{
    public static RouteGroupBuilder MapExportEndpoints(this RouteGroupBuilder api)
    {
        api.MapGet("/export", async (HttpContext http, IExportService service, CancellationToken ct) =>
        {
            var export = await service.ExportAsync(ct);
            var fileName = $"ideavault-backup-{export.ExportedAt:yyyy-MM-dd}.json";
            http.Response.Headers.ContentDisposition = $"attachment; filename=\"{fileName}\"";
            return TypedResults.Ok(export);
        })
        .WithTags("Export")
        .WithSummary("Full JSON backup of categories, ideas and sections.");

        return api;
    }
}
