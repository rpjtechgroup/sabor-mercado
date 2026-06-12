using SaborMercado.Web.Contracts.Community;
using SaborMercado.Web.Infrastructure;

namespace SaborMercado.Web.Features.Account;

public sealed class CommunityService(SaborMercadoApiClient api)
{
    public async Task<SharedObservationListResponse> ListObservationsAsync(
        Guid productId,
        CancellationToken cancellationToken = default)
    {
        var response = await api.SendAsync(
            HttpMethod.Get,
            $"/api/v1/shared-products/{productId}/observations",
            requireAuth: true,
            cancellationToken: cancellationToken);

        if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            throw new CommunityException("Produto não encontrado no catálogo colaborativo.");
        }

        if (!response.IsSuccessStatusCode)
        {
            var detail = await api.ReadErrorDetailAsync(response, cancellationToken);
            throw new CommunityException(detail ?? "Não foi possível carregar as observações.");
        }

        return await api.ReadJsonAsync<SharedObservationListResponse>(response, cancellationToken)
            ?? new SharedObservationListResponse(productId, string.Empty, []);
    }

    public async Task<VoteObservationResponse> VoteAsync(
        Guid observationId,
        int value,
        CancellationToken cancellationToken = default)
    {
        var response = await api.SendAsync(
            HttpMethod.Post,
            $"/api/v1/price-observations/{observationId}/vote",
            new VoteObservationRequest(value),
            requireAuth: true,
            cancellationToken: cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            var detail = await api.ReadErrorDetailAsync(response, cancellationToken);
            throw new CommunityException(detail ?? "Não foi possível registrar o voto.");
        }

        return await api.ReadJsonAsync<VoteObservationResponse>(response, cancellationToken)
            ?? throw new InvalidOperationException("Resposta de voto inválida.");
    }

    public async Task<VoteObservationResponse> RemoveVoteAsync(
        Guid observationId,
        CancellationToken cancellationToken = default)
    {
        var response = await api.SendAsync(
            HttpMethod.Delete,
            $"/api/v1/price-observations/{observationId}/vote",
            requireAuth: true,
            cancellationToken: cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            var detail = await api.ReadErrorDetailAsync(response, cancellationToken);
            throw new CommunityException(detail ?? "Não foi possível remover o voto.");
        }

        return await api.ReadJsonAsync<VoteObservationResponse>(response, cancellationToken)
            ?? throw new InvalidOperationException("Resposta de voto inválida.");
    }

    public async Task SubmitReportAsync(
        SubmitContributorReportRequest request,
        CancellationToken cancellationToken = default)
    {
        var response = await api.SendAsync(
            HttpMethod.Post,
            "/api/v1/contributor-reports",
            request,
            requireAuth: true,
            cancellationToken: cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            var detail = await api.ReadErrorDetailAsync(response, cancellationToken);
            throw new CommunityException(detail ?? "Não foi possível enviar a denúncia.");
        }
    }

    public async Task<AchievementListResponse> ListAchievementsAsync(
        CancellationToken cancellationToken = default)
    {
        var response = await api.SendAsync(
            HttpMethod.Get,
            "/api/v1/achievements",
            requireAuth: true,
            cancellationToken: cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            var detail = await api.ReadErrorDetailAsync(response, cancellationToken);
            throw new CommunityException(detail ?? "Não foi possível carregar as conquistas.");
        }

        return await api.ReadJsonAsync<AchievementListResponse>(response, cancellationToken)
            ?? new AchievementListResponse([]);
    }
}

public sealed class CommunityException(string message) : Exception(message);
