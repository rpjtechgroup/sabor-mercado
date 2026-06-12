using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using SaborMercado.Api.Tests.Fakes;
using SaborMercado.Modules.Recognition.Services;
using SaborMercado.Shared.Auth;

namespace SaborMercado.Api.Tests;

public class PremiumGateEndpointTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public PremiumGateEndpointTests(WebApplicationFactory<Program> factory) =>
        _factory = factory.WithWebHostBuilder(builder =>
        {
            var suffix = Guid.NewGuid().ToString("N");
            builder.UseSetting("ConnectionStrings:Identity", $"Data Source=premium-id-{suffix}.db");
            builder.UseSetting("ConnectionStrings:SharedCatalog", $"Data Source=premium-cat-{suffix}.db");
            builder.UseSetting("ConnectionStrings:Rewards", $"Data Source=premium-rew-{suffix}.db");
            builder.ConfigureServices(services =>
            {
                services.RemoveAll<IGeminiVisionClient>();
                services.AddSingleton<IGeminiVisionClient, StubGeminiVisionClient>();
            });
        });

    [Fact]
    public async Task SearchSharedProducts_WithoutUnlock_ReturnsPremiumRequired()
    {
        var client = await CreateAuthenticatedClientAsync();

        var response = await client.GetAsync("/api/v1/shared-products?query=leite");

        Assert.Equal(System.Net.HttpStatusCode.Forbidden, response.StatusCode);
        var body = await response.Content.ReadAsStringAsync();
        Assert.Contains("PREMIUM_REQUIRED", body, StringComparison.Ordinal);
    }

    [Fact]
    public async Task CompareMarkets_WithoutUnlock_ReturnsPremiumRequired()
    {
        var client = await CreateAuthenticatedClientAsync();
        var productId = Guid.NewGuid();

        var response = await client.GetAsync($"/api/v1/shared-products/{productId}/markets");

        Assert.Equal(System.Net.HttpStatusCode.Forbidden, response.StatusCode);
        var body = await response.Content.ReadAsStringAsync();
        Assert.Contains("PREMIUM_REQUIRED", body, StringComparison.Ordinal);
    }

    private async Task<HttpClient> CreateAuthenticatedClientAsync()
    {
        var client = _factory.CreateClient();
        var email = $"premium-{Guid.NewGuid():N}@test.local";
        var auth = await (await client.PostAsJsonAsync(
            "/api/v1/auth/register",
            new RegisterRequest(email, "password123"))).Content.ReadFromJsonAsync<AuthResponse>();

        Assert.NotNull(auth);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", auth.AccessToken);
        return client;
    }
}
