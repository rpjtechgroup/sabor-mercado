using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using SaborMercado.Api.Tests.Fakes;
using SaborMercado.Modules.Recognition.Services;
using SaborMercado.Shared.Auth;
using SaborMercado.Shared.SharedCatalog;

namespace SaborMercado.Api.Tests;

public class PriceOutlierEndpointTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public PriceOutlierEndpointTests(WebApplicationFactory<Program> factory) =>
        _factory = factory.WithWebHostBuilder(builder =>
        {
            var suffix = Guid.NewGuid().ToString("N");
            builder.UseSetting("ConnectionStrings:Identity", $"Data Source=outlier-id-{suffix}.db");
            builder.UseSetting("ConnectionStrings:SharedCatalog", $"Data Source=outlier-cat-{suffix}.db");
            builder.UseSetting("ConnectionStrings:Rewards", $"Data Source=outlier-rew-{suffix}.db");
            builder.ConfigureServices(services =>
            {
                services.RemoveAll<IGeminiVisionClient>();
                services.AddSingleton<IGeminiVisionClient, StubGeminiVisionClient>();
            });
        });

    [Fact]
    public async Task PostObservation_WithExtremePrice_ReturnsPriceOutlier()
    {
        var client = await CreateAuthenticatedClientAsync();
        var observedOn = DateOnly.FromDateTime(DateTime.UtcNow);

        for (var i = 0; i < 5; i++)
        {
            var baseline = CreateObservation($"Mercado Baseline {i}", 5.00m + (i * 0.01m), observedOn);
            var baselineResponse = await client.PostAsJsonAsync("/api/v1/price-observations", baseline);
            baselineResponse.EnsureSuccessStatusCode();
        }

        var outlier = CreateObservation("Mercado Outlier", 50.00m, observedOn);
        var response = await client.PostAsJsonAsync("/api/v1/price-observations", outlier);

        Assert.Equal(System.Net.HttpStatusCode.UnprocessableEntity, response.StatusCode);
        using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        Assert.Equal(ContributionErrorCodes.PriceOutlier, document.RootElement.GetProperty("code").GetString());
    }

    private static SubmitPriceObservationRequest CreateObservation(
        string marketName,
        decimal price,
        DateOnly observedOn) =>
        new(
            "Leite Integral",
            "Itambé",
            1m,
            "l",
            null,
            price,
            marketName,
            "São Paulo",
            "SP",
            observedOn);

    private async Task<HttpClient> CreateAuthenticatedClientAsync()
    {
        var client = _factory.CreateClient();
        var email = $"outlier-{Guid.NewGuid():N}@test.local";
        var auth = await (await client.PostAsJsonAsync(
            "/api/v1/auth/register",
            new RegisterRequest(email, "password123"))).Content.ReadFromJsonAsync<AuthResponse>();

        Assert.NotNull(auth);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", auth.AccessToken);
        return client;
    }
}
