using SaborMercado.Shared.Rewards;
using SaborMercado.Web.Infrastructure;
using SaborMercado.Web.Shared;

namespace SaborMercado.Web.Features.Gamification;

public sealed class GamificationRankingService(SaborMercadoApiClient api, TimeProvider clock)
{
    private readonly Dictionary<string, (RankingListResponse Data, DateTimeOffset ExpiresAt)> _cache = new(StringComparer.Ordinal);
    private readonly TimeSpan _cacheDuration = TimeSpan.FromMinutes(5);

    public async Task<RankingListResponse> GetRankingAsync(string rankingType, bool forceRefresh = false)
    {
        var now = clock.GetUtcNow();
        if (!forceRefresh &&
            _cache.TryGetValue(rankingType, out var cached) &&
            cached.ExpiresAt > now)
        {
            return cached.Data;
        }

        var response = await api.SendAsync(
            HttpMethod.Get,
            $"/api/v1/rankings/{rankingType}",
            requireAuth: true);

        if (!response.IsSuccessStatusCode)
        {
            var detail = await api.ReadErrorDetailAsync(response, CancellationToken.None);
            throw new GamificationException(detail ?? "Não foi possível carregar o ranking.");
        }

        var data = await api.ReadJsonAsync<RankingListResponse>(response, CancellationToken.None)
            ?? throw new GamificationException("Resposta de ranking inválida.");

        _cache[rankingType] = (data, now.Add(_cacheDuration));
        return data;
    }
}

public sealed class GamificationException(string message) : Exception(message);
