using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using SaborMercado.Api.Tests.Fakes;
using SaborMercado.Modules.Recognition.Services;
using Testcontainers.PostgreSql;

namespace SaborMercado.Api.Tests;

[Trait("Category", "Integration")]
public sealed class PostgreSqlMigrationTests : IAsyncLifetime
{
    private PostgreSqlContainer? _postgres;

    public async Task InitializeAsync()
    {
        try
        {
            _postgres = new PostgreSqlBuilder()
                .WithImage("postgres:16-alpine")
                .Build();
            await _postgres.StartAsync();
        }
        catch (ArgumentException ex) when (ex.ParamName == "DockerEndpointAuthConfig")
        {
            _postgres = null;
        }
    }

    public async Task DisposeAsync()
    {
        if (_postgres is not null)
        {
            await _postgres.DisposeAsync();
        }
    }

    [Fact]
    public async Task Startup_WithPostgreSqlProvider_AppliesMigrationsAndReadyzIsHealthy()
    {
        if (_postgres is null)
        {
            return;
        }

        var connectionString = _postgres.GetConnectionString();
        await using var factory = new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
        {
            builder.UseSetting("Database:Provider", "PostgreSQL");
            builder.UseSetting("ConnectionStrings:Identity", connectionString);
            builder.UseSetting("ConnectionStrings:SharedCatalog", connectionString);
            builder.UseSetting("ConnectionStrings:Rewards", connectionString);
            builder.UseSetting("ConnectionStrings:Recognition", connectionString);
            builder.ConfigureServices(services =>
            {
                services.RemoveAll<IGeminiVisionClient>();
                services.AddSingleton<IGeminiVisionClient, StubGeminiVisionClient>();
            });
        });

        var client = factory.CreateClient();
        var ready = await client.GetAsync("/readyz");

        Assert.Equal(System.Net.HttpStatusCode.OK, ready.StatusCode);
    }
}
