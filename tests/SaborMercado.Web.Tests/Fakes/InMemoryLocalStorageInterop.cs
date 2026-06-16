using SaborMercado.Web.Interop;

namespace SaborMercado.Web.Tests.Fakes;

public sealed class InMemoryLocalStorageInterop : ILocalStorageInterop
{
    private readonly Dictionary<string, string> _items = new(StringComparer.Ordinal);

    public ValueTask SetItemAsync(string key, string value)
    {
        _items[key] = value;
        return ValueTask.CompletedTask;
    }

    public ValueTask<string?> GetItemAsync(string key) =>
        ValueTask.FromResult(_items.TryGetValue(key, out var value) ? value : null);

    public ValueTask RemoveItemAsync(string key)
    {
        _items.Remove(key);
        return ValueTask.CompletedTask;
    }
}
