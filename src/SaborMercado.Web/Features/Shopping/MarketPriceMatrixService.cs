using System.Globalization;
using SaborMercado.Web.Domain.Catalog;
using SaborMercado.Web.Domain.Shopping;
using SaborMercado.Web.Shared;
using SaborMercado.Web.Storage;

namespace SaborMercado.Web.Features.Shopping;

public sealed class MarketPriceMatrixService(IShoppingStore store, TimeProvider clock)
{
    public async Task<IReadOnlyList<MarketMatrixRow>> GetMatrixAsync()
    {
        var cutoff = clock.GetUtcNow() - PurchaseHistoryService.HistoryWindow;
        var sessions = (await store.GetAllSessionsAsync())
            .Where(s => s.Status == SessionStatus.Finished &&
                        s.FinishedAt is not null &&
                        s.FinishedAt >= cutoff)
            .ToList();

        var observations = new List<(string ProductKey, string ProductName, string? VolumeLabel, string MarketName, decimal UnitPrice, DateOnly Date)>();

        foreach (var session in sessions)
        {
            var date = DateOnly.FromDateTime(session.FinishedAt!.Value.UtcDateTime);
            var items = await store.GetItemsAsync(session.Id);

            foreach (var item in items)
            {
                var market = ResolveMarketName(item, session);
                observations.Add((
                    ProductIdentity.GetLineKey(item.ProductId, item.ProductSnapshot),
                    item.ProductSnapshot.Name,
                    FormatUnit(item.ProductSnapshot),
                    market,
                    item.UnitPrice,
                    date));
            }
        }

        return observations
            .GroupBy(o => o.ProductKey)
            .Select(group =>
            {
                var sample = group.First();
                var cells = group
                    .GroupBy(o => o.MarketName, StringComparer.OrdinalIgnoreCase)
                    .Select(marketGroup =>
                    {
                        var latest = marketGroup.OrderByDescending(o => o.Date).First();
                        return new MarketPriceCell(latest.MarketName, latest.UnitPrice, latest.Date);
                    })
                    .OrderBy(c => c.MarketName, StringComparer.Create(MoneyFormat.Culture, CompareOptions.IgnoreCase))
                    .ToList();

                var best = cells.OrderBy(c => c.UnitPrice).FirstOrDefault();
                return new MarketMatrixRow(
                    sample.ProductName,
                    sample.VolumeLabel,
                    group.Key,
                    cells,
                    best?.MarketName,
                    best?.UnitPrice);
            })
            .OrderBy(r => r.ProductName, StringComparer.Create(MoneyFormat.Culture, CompareOptions.IgnoreCase))
            .ToList();
    }

    public static IReadOnlyList<string> GetMarketColumns(IReadOnlyList<MarketMatrixRow> rows) =>
        rows.SelectMany(r => r.Cells.Select(c => c.MarketName))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(m => m, StringComparer.Create(MoneyFormat.Culture, CompareOptions.IgnoreCase))
            .ToList();

    private static string ResolveMarketName(CartItem item, ShoppingSession session)
    {
        if (!string.IsNullOrWhiteSpace(item.StoreName))
        {
            return item.StoreName.Trim();
        }

        return string.IsNullOrWhiteSpace(session.MarketName) ? "—" : session.MarketName.Trim();
    }

    private static string? FormatUnit(ProductSnapshot snapshot)
    {
        if (snapshot.QuantityValue is not { } value)
        {
            return snapshot.QuantityUnit?.Label();
        }

        var formatted = value.ToString("0.##", MoneyFormat.Culture);
        var unit = snapshot.QuantityUnit?.Label();
        return unit is null ? formatted : $"{formatted} {unit}";
    }
}
