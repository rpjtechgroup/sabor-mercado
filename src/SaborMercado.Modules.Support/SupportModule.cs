using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SaborMercado.Modules.Support.Endpoints;
using SaborMercado.Modules.Support.Services;

namespace SaborMercado.Modules.Support;

public static class SupportModule
{
    public static IServiceCollection AddSupportModule(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddScoped<FeedbackEmailService>();
        return services;
    }

    public static IEndpointRouteBuilder MapSupportModule(this IEndpointRouteBuilder app) =>
        app.MapFeedbackEndpoints();
}
