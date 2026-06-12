using SaborMercado.Web.Domain.Shopping;
using SaborMercado.Web.Storage;

namespace SaborMercado.Web.Features.Premium;

public sealed class PremiumStatsService(IShoppingStore store)
{
    public async Task<SpendingStats> GetStatsAsync()
    {
        var sessions = await store.GetAllSessionsAsync();
        var finished = sessions
            .Where(s => s.Status == SessionStatus.Finished && s.FinishedAt is not null)
            .ToList();

        if (finished.Count == 0)
        {
            return new SpendingStats(0, 0m, 0m, 0m, []);
        }

        var totals = new List<decimal>();
        var monthlyTotals = new Dictionary<(int Year, int Month), List<decimal>>();

        foreach (var session in finished)
        {
            var items = await store.GetItemsAsync(session.Id);
            var total = items.Sum(i => i.Subtotal);
            totals.Add(total);

            var finishedAt = session.FinishedAt!.Value;
            var key = (finishedAt.Year, finishedAt.Month);
            if (!monthlyTotals.TryGetValue(key, out var bucket))
            {
                bucket = [];
                monthlyTotals[key] = bucket;
            }

            bucket.Add(total);
        }

        var monthly = monthlyTotals
            .OrderByDescending(kv => kv.Key.Year)
            .ThenByDescending(kv => kv.Key.Month)
            .Take(6)
            .Select(kv => new MonthlySpend(
                $"{kv.Key.Month:00}/{kv.Key.Year}",
                kv.Value.Count,
                kv.Value.Sum()))
            .ToList();

        return new SpendingStats(
            finished.Count,
            totals.Average(),
            totals.Max(),
            totals.Min(),
            monthly);
    }
}

public sealed record SpendingStats(
    int FinishedTrips,
    decimal AverageSpend,
    decimal HighestSpend,
    decimal LowestSpend,
    IReadOnlyList<MonthlySpend> RecentMonths);

public sealed record MonthlySpend(string Label, int Trips, decimal TotalSpend);
