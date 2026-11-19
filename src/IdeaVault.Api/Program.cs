using IdeaVault.Api.Auth;
using IdeaVault.Api.Endpoints;
using IdeaVault.Api.Extensions;
using IdeaVault.Infra;

var builder = WebApplication.CreateBuilder(args);

// Render (and most PaaS) tell the app which port to listen on through $PORT.
if (builder.Configuration["PORT"] is { Length: > 0 } port)
{
    builder.WebHost.UseUrls($"http://0.0.0.0:{port}");
}

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

app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();

// Serves the Blazor app with fingerprinted, pre-compressed assets (and the processed index.html).
app.MapStaticAssets();

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
