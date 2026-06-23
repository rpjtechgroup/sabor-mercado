using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;

namespace SaborMercado.Api.Tests.Infrastructure;

public static class ApiIntegrationFactory
{
    public static WebApplicationFactory<Program> WithIsolatedSqlite(
        this WebApplicationFactory<Program> factory,
        string prefix,
        Action<IWebHostBuilder>? configure = null)
    {
        var suffix = Guid.NewGuid().ToString("N");
        return factory.WithWebHostBuilder(builder =>
        {
            builder.UseSetting("ConnectionStrings:Identity", $"Data Source={prefix}-id-{suffix}.db");
            builder.UseSetting("ConnectionStrings:SharedCatalog", $"Data Source={prefix}-cat-{suffix}.db");
            builder.UseSetting("ConnectionStrings:Rewards", $"Data Source={prefix}-rew-{suffix}.db");
            builder.UseSetting("ConnectionStrings:Recognition", $"Data Source={prefix}-rec-{suffix}.db");
            configure?.Invoke(builder);
        });
    }

    public static WebApplicationFactory<Program> Create(
        string prefix,
        Action<IWebHostBuilder>? configure = null) =>
        new WebApplicationFactory<Program>().WithIsolatedSqlite(prefix, configure);
}
