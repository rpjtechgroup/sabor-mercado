using SaborMercado.Web.Contracts.SharedCatalog;
using SaborMercado.Web.Infrastructure;

namespace SaborMercado.Web.Features.Account;

public sealed class CollaborativeCatalogService(SaborMercadoApiClient api)
{
    public async Task<SharedProductSearchResponse> SearchAsync(
        string query,
        CancellationToken cancellationToken = default)
    {
        var encoded = Uri.EscapeDataString(query);
        var response = await api.SendAsync(
            HttpMethod.Get,
            $"/api/v1/shared-products?query={encoded}",
            body: null,
            requireAuth: true,
            cancellationToken: cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            var detail = await api.ReadErrorDetailAsync(response, cancellationToken);
            throw new CollaborativeCatalogException(detail ?? "Não foi possível buscar o catálogo colaborativo.");
        }

        return await api.ReadJsonAsync<SharedProductSearchResponse>(response, cancellationToken)
            ?? new SharedProductSearchResponse([]);
    }
}

public sealed class CollaborativeCatalogException(string message) : Exception(message);
