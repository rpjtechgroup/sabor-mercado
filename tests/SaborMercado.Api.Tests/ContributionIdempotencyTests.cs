using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using SaborMercado.Api.Tests.Fakes;
using SaborMercado.Api.Tests.Infrastructure;
using SaborMercado.Modules.Recognition.Services;
using SaborMercado.Shared.Auth;
using SaborMercado.Shared.SharedCatalog;

namespace SaborMercado.Api.Tests;

public class ContributionIdempotencyTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public ContributionIdempotencyTests(WebApplicationFactory<Program> factory) =>
        _factory = factory.WithIsolatedSqlite("idemp", builder =>
            builder.ConfigureServices(services =>
            {
                services.RemoveAll<IGeminiVisionClient>();
                services.AddSingleton<IGeminiVisionClient, StubGeminiVisionClient>();
            }));

    [Fact]
    public async Task PostObservation_WithSameIdempotencyKey_ReturnsSameResponse()
    {
        var client = _factory.CreateClient();
        var email = $"idemp-{Guid.NewGuid():N}@test.local";
        var auth = await (await client.PostAsJsonAsync(
            "/api/v1/auth/register",
            new RegisterRequest(email, "password123"))).Content.ReadFromJsonAsync<AuthResponse>();

        Assert.NotNull(auth);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", auth.AccessToken);

        var request = new SubmitPriceObservationRequest(
            "Café",
            "Melitta",
            500m,
            "g",
            null,
            12.99m,
            "Mercado Central",
            null,
            null,
            DateOnly.FromDateTime(DateTime.UtcNow));

        using var firstMessage = new HttpRequestMessage(HttpMethod.Post, "/api/v1/price-observations")
        {
            Content = JsonContent.Create(request),
        };
        firstMessage.Headers.TryAddWithoutValidation("Idempotency-Key", "share-1");

        using var secondMessage = new HttpRequestMessage(HttpMethod.Post, "/api/v1/price-observations")
        {
            Content = JsonContent.Create(request),
        };
        secondMessage.Headers.TryAddWithoutValidation("Idempotency-Key", "share-1");

        var first = await client.SendAsync(firstMessage);
        var second = await client.SendAsync(secondMessage);
        first.EnsureSuccessStatusCode();
        second.EnsureSuccessStatusCode();

        var firstBody = await first.Content.ReadFromJsonAsync<PriceObservationResponse>();
        var secondBody = await second.Content.ReadFromJsonAsync<PriceObservationResponse>();
        Assert.NotNull(firstBody);
        Assert.NotNull(secondBody);
        Assert.Equal(firstBody.ObservationId, secondBody.ObservationId);
    }

    [Fact]
    public async Task PostObservation_ReusedKeyWithDifferentPayload_Returns409()
    {
        var client = _factory.CreateClient();
        var email = $"idemp-conflict-{Guid.NewGuid():N}@test.local";
        var auth = await (await client.PostAsJsonAsync(
            "/api/v1/auth/register",
            new RegisterRequest(email, "password123"))).Content.ReadFromJsonAsync<AuthResponse>();

        Assert.NotNull(auth);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", auth.AccessToken);

        var firstRequest = new SubmitPriceObservationRequest(
            "Café",
            "Melitta",
            500m,
            "g",
            null,
            12.99m,
            "Mercado Central",
            null,
            null,
            DateOnly.FromDateTime(DateTime.UtcNow));

        var secondRequest = firstRequest with { Price = 13.99m };

        using var firstMessage = new HttpRequestMessage(HttpMethod.Post, "/api/v1/price-observations")
        {
            Content = JsonContent.Create(firstRequest),
        };
        firstMessage.Headers.TryAddWithoutValidation("Idempotency-Key", "share-conflict");

        using var secondMessage = new HttpRequestMessage(HttpMethod.Post, "/api/v1/price-observations")
        {
            Content = JsonContent.Create(secondRequest),
        };
        secondMessage.Headers.TryAddWithoutValidation("Idempotency-Key", "share-conflict");

        var first = await client.SendAsync(firstMessage);
        var second = await client.SendAsync(secondMessage);
        first.EnsureSuccessStatusCode();

        Assert.Equal(System.Net.HttpStatusCode.Conflict, second.StatusCode);
        var body = await second.Content.ReadAsStringAsync();
        Assert.Contains(ContributionErrorCodes.IdempotencyConflict, body, StringComparison.Ordinal);
    }
}
