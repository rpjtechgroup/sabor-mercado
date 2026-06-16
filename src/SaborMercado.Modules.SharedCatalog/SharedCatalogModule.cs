using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SaborMercado.Infrastructure.EntityFramework;
using SaborMercado.Modules.SharedCatalog.Data;
using SaborMercado.Modules.SharedCatalog.Endpoints;
using SaborMercado.Modules.SharedCatalog.Services;
using SaborMercado.Shared.StarterCatalog;

namespace SaborMercado.Modules.SharedCatalog;

public static class SharedCatalogModule
{
    public static IServiceCollection AddSharedCatalogModule(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddSingleton(TimeProvider.System);
        services.AddScoped<ContributionService>();
        services.AddScoped<SharedProductSearchService>();
        services.AddScoped<MarketComparisonService>();
        services.AddScoped<ContributorTrustService>();
        services.AddScoped<ObservationVoteService>();
        services.AddScoped<ContributorReportService>();
        services.AddScoped<SharedObservationQueryService>();
        services.AddSingleton<StarterCatalogProvider>();

        services.AddDbContextPool<SharedCatalogDbContext>(options =>
            DatabaseBootstrap.ConfigureDbContext<SharedCatalogDbContext>(
                options,
                configuration,
                "SharedCatalog",
                postgresSchema: "shared_catalog"));

        return services;
    }

    public static Task InitializeSharedCatalogModuleAsync(this WebApplication app) =>
        DatabaseBootstrap.InitializeModuleAsync<SharedCatalogDbContext>(app);

    public static IEndpointRouteBuilder MapSharedCatalogModule(this IEndpointRouteBuilder app) =>
        app.MapStarterCatalogEndpoints()
            .MapContributionEndpoints()
            .MapSharedProductEndpoints()
            .MapCommunityEndpoints()
            .MapSharedObservationEndpoints();
}
