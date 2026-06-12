using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using SaborMercado.Api.Tests.Fakes;
using SaborMercado.Modules.Recognition.Services;
using SaborMercado.Shared.Auth;
using SaborMercado.Shared.Rewards;
using SaborMercado.Shared.SharedCatalog;

namespace SaborMercado.Api.Tests;

public class ShareToUnlockEndpointTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public ShareToUnlockEndpointTests(WebApplicationFactory<Program> factory) =>
        _factory = factory.WithWebHostBuilder(builder =>
        {
            var suffix = Guid.NewGuid().ToString("N");
            builder.UseSetting("ConnectionStrings:Identity", $"Data Source=sharetest-id-{suffix}.db");
            builder.UseSetting("ConnectionStrings:SharedCatalog", $"Data Source=sharetest-cat-{suffix}.db");
            builder.UseSetting("ConnectionStrings:Rewards", $"Data Source=sharetest-rew-{suffix}.db");
            builder.ConfigureServices(services =>
            {
                services.RemoveAll<IGeminiVisionClient>();
                services.AddSingleton<IGeminiVisionClient, StubGeminiVisionClient>();
            });
        });

    [Fact]
    public async Task RegisterShareAndUnlock_FlowGrantsCredits()
    {
        var client = _factory.CreateClient();
        var email = $"user-{Guid.NewGuid():N}@test.local";

        var register = await client.PostAsJsonAsync(
            "/api/v1/auth/register",
            new RegisterRequest(email, "password123"));

        register.EnsureSuccessStatusCode();
        var auth = await register.Content.ReadFromJsonAsync<AuthResponse>();
        Assert.NotNull(auth);

        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", auth.AccessToken);

        var observation = new SubmitPriceObservationRequest(
            "Leite Integral",
            "Itambé",
            1m,
            "l",
            null,
            4.99m,
            "Mercado Teste",
            "São Paulo",
            "SP",
            DateOnly.FromDateTime(DateTime.UtcNow));

        var share = await client.PostAsJsonAsync("/api/v1/price-observations", observation);
        share.EnsureSuccessStatusCode();

        var shareResult = await share.Content.ReadFromJsonAsync<PriceObservationResponse>();
        Assert.NotNull(shareResult);
        Assert.True(shareResult.CreditsGranted >= 1);

        var credits = await client.GetFromJsonAsync<CreditsResponse>("/api/v1/credits");
        Assert.NotNull(credits);
        Assert.True(credits.Balance >= shareResult.CreditsGranted);

        var unlock = await client.PostAsJsonAsync(
            "/api/v1/unlocks",
            new UnlockRequest(PremiumFeatureCodes.ExportCsv));

        if (credits.Balance >= 15)
        {
            unlock.EnsureSuccessStatusCode();
            var unlockResult = await unlock.Content.ReadFromJsonAsync<UnlockResponse>();
            Assert.NotNull(unlockResult);
            Assert.Equal(PremiumFeatureCodes.ExportCsv, unlockResult.FeatureCode);
        }
        else
        {
            Assert.Equal(System.Net.HttpStatusCode.UnprocessableEntity, unlock.StatusCode);
        }
    }
}
