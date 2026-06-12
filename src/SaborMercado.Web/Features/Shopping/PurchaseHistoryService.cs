using System.Globalization;
using SaborMercado.Web.Domain.Catalog;
using SaborMercado.Web.Domain.Shopping;
using SaborMercado.Web.Shared;
using SaborMercado.Web.Storage;

namespace SaborMercado.Web.Features.Shopping;

public sealed class PurchaseHistoryService(IShoppingStore store, TimeProvider clock)
{
    public static readonly TimeSpan HistoryWindow = TimeSpan.FromDays(90);

    public async Task<IReadOnlyList<FinishedSessionSummary>> GetRecentSessionsAsync()
    {
        var sessions = await GetFinishedSessionsInWindowAsync();
        var summaries = new List<FinishedSessionSummary>();

        foreach (var session in sessions.OrderByDescending(s => s.FinishedAt))
        {
            var items = await store.GetItemsAsync(session.Id);
            summaries.Add(new FinishedSessionSummary(
                session.Id,
                DateOnly.FromDateTime(session.FinishedAt!.Value.UtcDateTime),
                session.MarketName,
                items.Sum(i => i.Subtotal),
                items.Count));
        }

        return summaries;
    }

    public async Task<IReadOnlyList<PurchaseLineRow>> GetSessionGridAsync(Guid sessionId)
    {
        var allLines = await BuildAllLinesWithTrendsAsync();
        return allLines
            .Where(l => l.SessionId == sessionId)
            .OrderBy(l => l.ProductName, StringComparer.Create(MoneyFormat.Culture, CompareOptions.IgnoreCase))
            .ToList();
    }

    public async Task<IReadOnlyList<PurchaseLineRow>> GetConsolidatedGridAsync()
    {
        var allLines = await BuildAllLinesWithTrendsAsync();
        return allLines
            .OrderBy(l => l.ProductName, StringComparer.Create(MoneyFormat.Culture, CompareOptions.IgnoreCase))
            .ThenBy(l => l.PurchaseDate)
            .ToList();
    }

    private async Task<List<PurchaseLineRow>> BuildAllLinesWithTrendsAsync()
    {
        var sessions = await GetFinishedSessionsInWindowAsync();
        var chronological = new List<PurchaseLineRow>();

        foreach (var session in sessions.OrderBy(s => s.FinishedAt))
        {
            var items = await store.GetItemsAsync(session.Id);
            var purchaseDate = DateOnly.FromDateTime(session.FinishedAt!.Value.UtcDateTime);

            foreach (var item in items)
            {
                chronological.Add(ToRow(item, session, purchaseDate));
            }
        }

        return ApplyTrends(chronological);
    }

    private async Task<List<ShoppingSession>> GetFinishedSessionsInWindowAsync()
    {
        var cutoff = clock.GetUtcNow() - HistoryWindow;
        var sessions = await store.GetAllSessionsAsync();
        return sessions
            .Where(s => s.Status == SessionStatus.Finished &&
                        s.FinishedAt is not null &&
                        s.FinishedAt >= cutoff)
            .ToList();
    }

    private static List<PurchaseLineRow> ApplyTrends(List<PurchaseLineRow> chronologicalLines)
    {
        var result = new List<PurchaseLineRow>(chronologicalLines.Count);
        var lastPrice = new Dictionary<string, decimal>(StringComparer.Ordinal);

        foreach (var line in chronologicalLines)
        {
            decimal? delta = null;
            PriceTrend trend;
            if (!lastPrice.TryGetValue(line.ProductKey, out var previous))
            {
                trend = PriceTrend.None;
            }
            else
            {
                delta = line.UnitPrice - previous;
                if (line.UnitPrice < previous)
                {
                    trend = PriceTrend.Cheaper;
                }
                else if (line.UnitPrice > previous)
                {
                    trend = PriceTrend.MoreExpensive;
                }
                else
                {
                    trend = PriceTrend.None;
                }
            }

            lastPrice[line.ProductKey] = line.UnitPrice;
            result.Add(line with { Trend = trend, DeltaVsPrevious = delta });
        }

        return result;
    }

    private static PurchaseLineRow ToRow(
        CartItem item,
        ShoppingSession session,
        DateOnly purchaseDate) =>
        new(
            session.Id,
            purchaseDate,
            session.MarketName,
            item.ProductSnapshot.Name,
            FormatUnit(item.ProductSnapshot),
            item.UnitPrice,
            null,
            PriceTrend.None,
            ProductIdentity.GetLineKey(item.ProductId, item.ProductSnapshot));

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
