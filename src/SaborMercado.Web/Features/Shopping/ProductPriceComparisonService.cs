using System.Globalization;
using SaborMercado.Web.Domain.Catalog;
using SaborMercado.Web.Domain.Shopping;
using SaborMercado.Web.Shared;
using SaborMercado.Web.Storage;

namespace SaborMercado.Web.Features.Shopping;

public sealed class ProductPriceComparisonService(IShoppingStore store, TimeProvider clock)
{
    public static readonly TimeSpan HistoryWindow = TimeSpan.FromDays(90);

    public async Task<IReadOnlyList<ProductComparisonRow>> GetComparisonRowsAsync()
    {
        var cutoff = clock.GetUtcNow() - HistoryWindow;
        var sessions = (await store.GetAllSessionsAsync())
            .Where(s => s.Status == SessionStatus.Finished &&
                        s.FinishedAt is not null &&
                        s.FinishedAt >= cutoff)
            .ToList();

        var observations = new List<(
            string ProductKey,
            string ProductName,
            string MarketName,
            decimal UnitPrice,
            DateOnly Date)>();

        foreach (var session in sessions)
        {
            var date = DateOnly.FromDateTime(session.FinishedAt!.Value.UtcDateTime);
            var items = await store.GetItemsAsync(session.Id);

            foreach (var item in items)
            {
                observations.Add((
                    ProductIdentity.GetLineKey(item.ProductId, item.ProductSnapshot),
                    item.ProductSnapshot.Name,
                    ResolveMarketName(item, session),
                    item.UnitPrice,
                    date));
            }
        }

        return observations
            .GroupBy(o => o.ProductKey)
            .Select(group =>
            {
                var bestPrice = group.Min(o => o.UnitPrice);
                var worstPrice = group.Max(o => o.UnitPrice);
                var bestObservation = group
                    .Where(o => o.UnitPrice == bestPrice)
                    .OrderByDescending(o => o.Date)
                    .First();

                return new ProductComparisonRow(
                    bestObservation.ProductName,
                    bestObservation.MarketName,
                    bestPrice,
                    worstPrice,
                    bestObservation.Date,
                    group.Key);
            })
            .OrderBy(r => r.ProductName, StringComparer.Create(MoneyFormat.Culture, CompareOptions.IgnoreCase))
            .ToList();
    }

    private static string ResolveMarketName(CartItem item, ShoppingSession session)
    {
        if (!string.IsNullOrWhiteSpace(item.StoreName))
        {
            return item.StoreName.Trim();
        }

        return string.IsNullOrWhiteSpace(session.MarketName) ? "—" : session.MarketName.Trim();
    }
}
