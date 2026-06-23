using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using SaborMercado.Api.Tests.Fakes;
using SaborMercado.Api.Tests.Infrastructure;
using SaborMercado.Modules.Recognition.Services;
using SaborMercado.Shared.Auth;
using SaborMercado.Shared.Community;
using SaborMercado.Shared.Rewards;
using SaborMercado.Shared.SharedCatalog;

namespace SaborMercado.Api.Tests;

public class CommunityTrustEndpointTests
{
    private WebApplicationFactory<Program> CreateFactory() =>
        ApiIntegrationFactory.Create("trust", builder =>
            builder.ConfigureServices(services =>
            {
                services.RemoveAll<IGeminiVisionClient>();
                services.AddSingleton<IGeminiVisionClient, StubGeminiVisionClient>();
            }));

    [Fact]
    public async Task Vote_OnOtherUserObservation_UpdatesNetScore()
    {
        var factory = CreateFactory();
        var contributor = await CreateAuthenticatedClientAsync(factory);
        var voter = await CreateAuthenticatedClientAsync(factory);

        var productId = await ShareAndGetProductIdAsync(contributor, "Arroz Tipo 1");
        await UnlockCollaborativeAsync(voter);

        var observations = await voter.GetFromJsonAsync<SharedObservationListResponse>(
            $"/api/v1/shared-products/{productId}/observations");

        Assert.NotNull(observations);
        var observationId = Assert.Single(observations.Observations).ObservationId;

        var vote = await voter.PostAsJsonAsync(
            $"/api/v1/price-observations/{observationId}/vote",
            new VoteObservationRequest(1));

        vote.EnsureSuccessStatusCode();
        var voteResult = await vote.Content.ReadFromJsonAsync<VoteObservationResponse>();
        Assert.NotNull(voteResult);
        Assert.Equal(1, voteResult.NetScore);
        Assert.Equal(1, voteResult.CurrentUserVote);
    }

    [Fact]
    public async Task Vote_OnOwnObservation_ReturnsSelfVoteNotAllowed()
    {
        var factory = CreateFactory();
        var client = await CreateAuthenticatedClientAsync(factory);
        var productId = await ShareAndGetProductIdAsync(client, "Feijão Preto");
        await UnlockCollaborativeAsync(client);

        var observations = await client.GetFromJsonAsync<SharedObservationListResponse>(
            $"/api/v1/shared-products/{productId}/observations");

        Assert.NotNull(observations);
        var observationId = Assert.Single(observations.Observations).ObservationId;

        var vote = await client.PostAsJsonAsync(
            $"/api/v1/price-observations/{observationId}/vote",
            new VoteObservationRequest(1));

        Assert.Equal(HttpStatusCode.Forbidden, vote.StatusCode);
        var body = await vote.Content.ReadAsStringAsync();
        Assert.Contains(CommunityErrorCodes.SelfVoteNotAllowed, body, StringComparison.Ordinal);
    }

    [Fact]
    public async Task Downvotes_HideObservation_WhenNetScoreAtOrBelowThreshold()
    {
        var factory = CreateFactory();
        var contributor = await CreateAuthenticatedClientAsync(factory);
        var voters = new[]
        {
            await CreateAuthenticatedClientAsync(factory),
            await CreateAuthenticatedClientAsync(factory),
            await CreateAuthenticatedClientAsync(factory),
        };

        var productId = await ShareAndGetProductIdAsync(contributor, "Óleo de Soja");
        Guid observationId = Guid.Empty;
        foreach (var voter in voters)
        {
            await UnlockCollaborativeAsync(voter);
            var list = await voter.GetFromJsonAsync<SharedObservationListResponse>(
                $"/api/v1/shared-products/{productId}/observations");
            Assert.NotNull(list);
            observationId = Assert.Single(list.Observations).ObservationId;

            var vote = await voter.PostAsJsonAsync(
                $"/api/v1/price-observations/{observationId}/vote",
                new VoteObservationRequest(-1));
            vote.EnsureSuccessStatusCode();
        }

        var after = await voters[0].GetFromJsonAsync<SharedObservationListResponse>(
            $"/api/v1/shared-products/{productId}/observations");

        Assert.NotNull(after);
        Assert.Empty(after.Observations);
    }

    [Fact]
    public async Task Report_ValidReason_ReturnsAccepted()
    {
        var factory = CreateFactory();
        var target = await CreateAuthenticatedClientAsync(factory);
        var reporter = await CreateAuthenticatedClientAsync(factory);

        var productId = await ShareAndGetProductIdAsync(target, "Leite Desnatado");
        await UnlockCollaborativeAsync(reporter);

        var list = await reporter.GetFromJsonAsync<SharedObservationListResponse>(
            $"/api/v1/shared-products/{productId}/observations");

        Assert.NotNull(list);
        var observation = Assert.Single(list.Observations);

        var report = await reporter.PostAsJsonAsync(
            "/api/v1/contributor-reports",
            new SubmitContributorReportRequest(
                observation.Contributor.PseudonymId,
                observation.ObservationId,
                ReportReasons.MisleadingPrice,
                null));

        Assert.Equal(HttpStatusCode.Accepted, report.StatusCode);
    }

    [Fact]
    public async Task Report_OtherWithoutDetails_ReturnsBadRequest()
    {
        var factory = CreateFactory();
        var target = await CreateAuthenticatedClientAsync(factory);
        var reporter = await CreateAuthenticatedClientAsync(factory);

        var productId = await ShareAndGetProductIdAsync(target, "Café Torrado");
        await UnlockCollaborativeAsync(reporter);

        var list = await reporter.GetFromJsonAsync<SharedObservationListResponse>(
            $"/api/v1/shared-products/{productId}/observations");

        Assert.NotNull(list);
        var observation = Assert.Single(list.Observations);

        var report = await reporter.PostAsJsonAsync(
            "/api/v1/contributor-reports",
            new SubmitContributorReportRequest(
                observation.Contributor.PseudonymId,
                observation.ObservationId,
                ReportReasons.Other,
                null));

        Assert.Equal(HttpStatusCode.BadRequest, report.StatusCode);
    }

    [Fact]
    public async Task ThreeDistinctReports_RestrictsContributorOnNextShare()
    {
        var factory = CreateFactory();
        var (target, targetPseudonym) = await CreateAuthenticatedClientWithPseudonymAsync(factory);

        var reporters = new[]
        {
            await CreateAuthenticatedClientAsync(factory),
            await CreateAuthenticatedClientAsync(factory),
            await CreateAuthenticatedClientAsync(factory),
        };

        foreach (var reporter in reporters)
        {
            var report = await reporter.PostAsJsonAsync(
                "/api/v1/contributor-reports",
                new SubmitContributorReportRequest(targetPseudonym, null, ReportReasons.Spam, null));
            Assert.Equal(HttpStatusCode.Accepted, report.StatusCode);
        }

        var share = await target.PostAsJsonAsync(
            "/api/v1/price-observations",
            BuildObservation("Macarrão", 3.49m));

        Assert.Equal(HttpStatusCode.Forbidden, share.StatusCode);
        var body = await share.Content.ReadAsStringAsync();
        Assert.Contains(CommunityErrorCodes.ContributorRestricted, body, StringComparison.Ordinal);
    }

    [Fact]
    public async Task FirstContribution_UnlocksAchievement()
    {
        var factory = CreateFactory();
        var client = await CreateAuthenticatedClientAsync(factory);
        await ShareAndGetProductIdAsync(client, "Açúcar Cristal");

        var achievements = await client.GetFromJsonAsync<AchievementListResponse>("/api/v1/achievements");
        Assert.NotNull(achievements);
        Assert.Contains(achievements.Items, a => a.Code == AchievementCodes.FirstContribution);
    }

    private static SubmitPriceObservationRequest BuildObservation(string productName, decimal price) =>
        new(
            productName,
            null,
            1m,
            "kg",
            null,
            price,
            "Mercado Teste",
            "São Paulo",
            "SP",
            DateOnly.FromDateTime(DateTime.UtcNow));

    private async Task<Guid> ShareAndGetProductIdAsync(HttpClient contributor, string productName)
    {
        var share = await contributor.PostAsJsonAsync(
            "/api/v1/price-observations",
            BuildObservation(productName, 9.99m));

        share.EnsureSuccessStatusCode();

        await UnlockCollaborativeAsync(contributor);

        var search = await contributor.GetFromJsonAsync<SharedProductSearchResponse>(
            $"/api/v1/shared-products?query={Uri.EscapeDataString(productName)}");

        Assert.NotNull(search);
        var item = Assert.Single(search.Items);
        return item.ProductId;
    }

    private static async Task UnlockCollaborativeAsync(HttpClient client)
    {
        for (var attempt = 0; attempt < 20; attempt++)
        {
            var credits = await client.GetFromJsonAsync<CreditsResponse>("/api/v1/credits");
            if (credits?.ActiveUnlocks.Any(u => u.FeatureCode == PremiumFeatureCodes.CollaborativePriceHistory) == true)
            {
                return;
            }

            if (credits is { Balance: >= 10 })
            {
                var unlock = await client.PostAsJsonAsync(
                    "/api/v1/unlocks",
                    new UnlockRequest(PremiumFeatureCodes.CollaborativePriceHistory));

                if (unlock.IsSuccessStatusCode)
                {
                    return;
                }
            }

            var share = await client.PostAsJsonAsync(
                "/api/v1/price-observations",
                BuildObservation($"Produto Extra {Guid.NewGuid():N}", 5.99m + attempt));

            if (!share.IsSuccessStatusCode)
            {
                break;
            }
        }

        throw new InvalidOperationException("Não foi possível desbloquear o histórico colaborativo nos testes.");
    }

    private async Task<(HttpClient Client, Guid PseudonymId)> CreateAuthenticatedClientWithPseudonymAsync(
        WebApplicationFactory<Program> factory)
    {
        var client = factory.CreateClient();
        var email = $"trust-{Guid.NewGuid():N}@test.local";
        var register = await client.PostAsJsonAsync(
            "/api/v1/auth/register",
            new RegisterRequest(email, "password123"));

        register.EnsureSuccessStatusCode();
        var auth = await register.Content.ReadFromJsonAsync<AuthResponse>();
        Assert.NotNull(auth);

        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", auth.AccessToken);

        return (client, auth.PseudonymId);
    }

    private async Task<HttpClient> CreateAuthenticatedClientAsync(WebApplicationFactory<Program> factory)
    {
        var (client, _) = await CreateAuthenticatedClientWithPseudonymAsync(factory);
        return client;
    }

}
