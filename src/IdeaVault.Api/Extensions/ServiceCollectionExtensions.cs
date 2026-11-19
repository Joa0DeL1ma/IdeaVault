using System.Text.Json.Serialization;
using IdeaVault.Api.ErrorHandling;
using IdeaVault.Core.Interfaces;
using IdeaVault.Core.Services;
using Microsoft.OpenApi;

namespace IdeaVault.Api.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddSingleton(TimeProvider.System);
        services.AddScoped<ICategoryService, CategoryService>();
        services.AddScoped<IIdeaService, IdeaService>();
        services.AddScoped<IIdeaSectionService, IdeaSectionService>();
        services.AddScoped<IExportService, ExportService>();
        return services;
    }

    public static IServiceCollection AddWebApi(this IServiceCollection services)
    {
        services.ConfigureHttpJsonOptions(json =>
            json.SerializerOptions.Converters.Add(new JsonStringEnumConverter()));

        services.AddProblemDetails();
        services.AddExceptionHandler<DomainExceptionHandler>();
        services.AddHealthChecks();

        services.AddOpenApi(options => options.AddDocumentTransformer((document, _, _) =>
        {
            // Lets Swagger UI send the JWT ("Authorize" button).
            document.Components ??= new OpenApiComponents();
            document.Components.SecuritySchemes ??= new Dictionary<string, IOpenApiSecurityScheme>();
            document.Components.SecuritySchemes["Bearer"] = new OpenApiSecurityScheme
            {
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT",
            };
            document.Security = [new OpenApiSecurityRequirement { [new OpenApiSecuritySchemeReference("Bearer", document)] = [] }];
            return Task.CompletedTask;
        }));

        return services;
    }
}
