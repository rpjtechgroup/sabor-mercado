using System.Collections.Concurrent;

namespace SaborMercado.Modules.Recognition.Services;

public sealed class InMemoryRecognitionQuotaStore : IRecognitionQuotaStore
{
    private readonly ConcurrentDictionary<string, int> _counts = new();

    public bool TryConsumeGlobal(DateOnly day, int limit) =>
        TryConsume($"global:{day:yyyyMMdd}", limit);

    public bool TryConsumeClient(DateOnly day, string clientKey, int limit) =>
        TryConsume($"client:{clientKey}:{day:yyyyMMdd}", limit);

    private bool TryConsume(string key, int limit)
    {
        var updated = _counts.AddOrUpdate(key, 1, static (_, current) => current + 1);
        if (updated > limit)
        {
            _counts.TryUpdate(key, updated - 1, updated);
            return false;
        }

        return true;
    }
}
