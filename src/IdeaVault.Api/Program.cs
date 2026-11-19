using IdeaVault.Api.Extensions;
using IdeaVault.Infra;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("Default")
    ?? throw new InvalidOperationException("Missing connection string 'ConnectionStrings__Default'.");

builder.Services.AddInfrastructure(connectionString);
builder.Services.AddOpenApi();

var app = builder.Build();

await app.InitializeDatabaseAsync();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseWebAssemblyDebugging();
}

app.UseBlazorFrameworkFiles();
app.UseStaticFiles();

app.MapGet("/health", () => Results.Ok(new { status = "healthy" }));
app.MapFallbackToFile("index.html");

await app.RunAsync();
