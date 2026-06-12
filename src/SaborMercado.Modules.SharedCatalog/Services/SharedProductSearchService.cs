using Microsoft.EntityFrameworkCore;
using SaborMercado.Modules.SharedCatalog.Data;
using SaborMercado.Modules.SharedCatalog.Domain;
using SaborMercado.Shared.SharedCatalog;

namespace SaborMercado.Modules.SharedCatalog.Services;

public sealed class SharedProductSearchService(SharedCatalogDbContext db)
{
    public async Task<SharedProductSearchResponse> SearchAsync(
        string query,
        int limit,
        CancellationToken cancellationToken)
    {
        var normalized = ProductNormalizer.NormalizeName(query);
        if (normalized.Length < 2)
        {
            return new SharedProductSearchResponse([]);
        }

        var products = await db.SharedProducts
            .AsNoTracking()
            .Where(p => p.NormalizedName.Contains(normalized))
            .OrderBy(p => p.NormalizedName)
            .Take(limit)
            .ToListAsync(cancellationToken);

        var productIds = products.Select(p => p.Id).ToList();
        var observations = await db.PriceObservations
            .AsNoTracking()
            .Include(o => o.Market)
            .Where(o => productIds.Contains(o.SharedProductId) && o.Status == ObservationStatus.Accepted)
            .ToListAsync(cancellationToken);

        var latestByProduct = observations
            .GroupBy(o => o.SharedProductId)
            .ToDictionary(
                g => g.Key,
                g => g.OrderByDescending(o => o.ObservedOn).First());

        var items = products.Select(product =>
        {
            latestByProduct.TryGetValue(product.Id, out var latest);
            return new SharedProductSummaryDto(
                product.Id,
                product.NormalizedName,
                product.Brand,
                product.Ean,
                latest?.Price,
                latest?.Market.Name,
                latest?.ObservedOn);
        }).ToList();

        return new SharedProductSearchResponse(items);
    }
}
