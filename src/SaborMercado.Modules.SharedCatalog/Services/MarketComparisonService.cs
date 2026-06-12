using Microsoft.EntityFrameworkCore;
using SaborMercado.Modules.SharedCatalog.Data;
using SaborMercado.Shared.SharedCatalog;

namespace SaborMercado.Modules.SharedCatalog.Services;

public sealed class MarketComparisonService(SharedCatalogDbContext db)
{
    public async Task<MarketPriceComparisonResponse?> GetMarketPricesAsync(
        Guid productId,
        CancellationToken cancellationToken)
    {
        var product = await db.SharedProducts
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == productId, cancellationToken);

        if (product is null)
        {
            return null;
        }

        var observations = await db.PriceObservations
            .AsNoTracking()
            .Include(o => o.Market)
            .Where(o => o.SharedProductId == productId && o.Status == ObservationStatus.Accepted)
            .OrderByDescending(o => o.ObservedOn)
            .Take(500)
            .ToListAsync(cancellationToken);

        var markets = observations
            .GroupBy(o => o.MarketId)
            .Select(group =>
            {
                var latest = group.OrderByDescending(o => o.ObservedOn).First();
                return new MarketPriceDto(
                    latest.Market.Name,
                    latest.Market.City,
                    latest.Price,
                    latest.ObservedOn);
            })
            .OrderBy(m => m.Price)
            .ToList();

        return new MarketPriceComparisonResponse(product.Id, product.NormalizedName, markets);
    }
}
