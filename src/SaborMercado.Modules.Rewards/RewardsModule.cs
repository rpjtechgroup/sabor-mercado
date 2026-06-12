using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SaborMercado.Infrastructure.EntityFramework;
using SaborMercado.Modules.Rewards.Data;
using SaborMercado.Modules.Rewards.Endpoints;
using SaborMercado.Modules.Rewards.Services;
using SaborMercado.Shared.Rewards;

namespace SaborMercado.Modules.Rewards;

public static class RewardsModule
{
    public static IServiceCollection AddRewardsModule(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddSingleton(TimeProvider.System);
        services.AddScoped<RewardsService>();
        services.AddScoped<IContributionRewardService, ContributionRewardService>();
        services.AddScoped<IPremiumAccessService, PremiumAccessService>();

        services.AddDbContextPool<RewardsDbContext>(options =>
            DatabaseBootstrap.ConfigureDbContext<RewardsDbContext>(
                options,
                configuration,
                "Rewards",
                postgresSchema: "rewards"));

        return services;
    }

    public static Task InitializeRewardsModuleAsync(this WebApplication app) =>
        DatabaseBootstrap.InitializeModuleAsync<RewardsDbContext>(app);

    public static IEndpointRouteBuilder MapRewardsModule(this IEndpointRouteBuilder app) =>
        app.MapRewardsEndpoints();
}
