using IdeaVault.Api.Auth;
using IdeaVault.Api.Endpoints;
using IdeaVault.Api.Extensions;
using IdeaVault.Infra;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("Default")
    ?? throw new InvalidOperationException("Missing connection string 'ConnectionStrings__Default'.");

builder.Services
    .AddInfrastructure(connectionString)
    .AddApplicationServices()
    .AddSingleUserAuth()
    .AddWebApi();

var app = builder.Build();

await app.InitializeDatabaseAsync();

app.UseExceptionHandler();
app.UseStatusCodePages();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwaggerUI(options => options.SwaggerEndpoint("/openapi/v1.json", "IdeaVault API"));
    app.UseWebAssemblyDebugging();
}

app.UseBlazorFrameworkFiles();
app.UseStaticFiles();

app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();

app.MapHealthChecks("/health");
app.MapAuthEndpoints();

app.MapGroup("/api")
    .RequireAuthorization()
    .MapCategoryEndpoints()
    .MapIdeaEndpoints()
    .MapSectionEndpoints()
    .MapExportEndpoints();

// Unknown /api routes must 404 instead of falling through to the Blazor app.
app.Map("/api/{**path}", () => Results.Problem(statusCode: StatusCodes.Status404NotFound));
app.MapFallbackToFile("index.html");

await app.RunAsync();
