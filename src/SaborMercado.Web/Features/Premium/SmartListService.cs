using SaborMercado.Web.Domain.Shopping;
using SaborMercado.Web.Storage;

namespace SaborMercado.Web.Features.Premium;

public sealed class SmartListService(IShoppingStore store)
{
    public async Task<IReadOnlyList<SmartListItem>> GetSuggestionsAsync(int limit = 10)
    {
        var sessions = await store.GetAllSessionsAsync();
        var finishedIds = sessions
            .Where(s => s.Status == SessionStatus.Finished)
            .Select(s => s.Id)
            .ToHashSet();

        var counts = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

        foreach (var sessionId in finishedIds)
        {
            var items = await store.GetItemsAsync(sessionId);
            foreach (var item in items)
            {
                var key = item.ProductSnapshot.Name.Trim();
                counts.TryGetValue(key, out var count);
                counts[key] = count + item.Quantity;
            }
        }

        return counts
            .OrderByDescending(pair => pair.Value)
            .Take(limit)
            .Select(pair => new SmartListItem(pair.Key, pair.Value))
            .ToList();
    }
}

public sealed record SmartListItem(string ProductName, int TimesBought);
