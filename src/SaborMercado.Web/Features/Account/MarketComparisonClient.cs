using SaborMercado.Web.Contracts.SharedCatalog;
using SaborMercado.Web.Infrastructure;

namespace SaborMercado.Web.Features.Account;

public sealed class MarketComparisonClient(SaborMercadoApiClient api)
{
    public async Task<MarketPriceComparisonResponse?> GetAsync(
        Guid productId,
        CancellationToken cancellationToken = default)
    {
        var response = await api.SendAsync(
            HttpMethod.Get,
            $"/api/v1/shared-products/{productId}/markets",
            body: null,
            requireAuth: true,
            cancellationToken: cancellationToken);

        if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return null;
        }

        if (!response.IsSuccessStatusCode)
        {
            var detail = await api.ReadErrorDetailAsync(response, cancellationToken);
            throw new CollaborativeCatalogException(detail ?? "Não foi possível comparar mercados.");
        }

        return await api.ReadJsonAsync<MarketPriceComparisonResponse>(response, cancellationToken);
    }
}
