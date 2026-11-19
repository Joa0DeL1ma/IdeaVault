using IdeaVault.Core.Interfaces;
using IdeaVault.Infra.Data;
using IdeaVault.Infra.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace IdeaVault.Infra;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, string connectionString)
    {
        services.AddDbContext<IdeaVaultDbContext>(options =>
            options.UseNpgsql(ConnectionStringNormalizer.Normalize(connectionString), npgsql => npgsql.EnableRetryOnFailure()));

        services.AddScoped<ICategoryRepository, CategoryRepository>();
        services.AddScoped<IIdeaRepository, IdeaRepository>();

        return services;
    }
}
