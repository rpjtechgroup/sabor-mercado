using SaborMercado.Web.Domain.Shopping;
using SaborMercado.Web.Storage;

namespace SaborMercado.Web.Tests;

internal sealed class FakeShoppingStore : IShoppingStore
{
    private readonly List<ShoppingSession> _sessions = [];
    private readonly List<CartItem> _items = [];

    public IReadOnlyList<ShoppingSession> Sessions => _sessions;

    public IReadOnlyList<CartItem> Items => _items;

    public void AddSession(ShoppingSession session) => _sessions.Add(session);

    public void AddItem(CartItem item) => _items.Add(item);

    public Task<List<ShoppingSession>> GetAllSessionsAsync() =>
        Task.FromResult(_sessions.ToList());

    public Task SaveSessionAsync(ShoppingSession session)
    {
        var index = _sessions.FindIndex(s => s.Id == session.Id);
        if (index >= 0)
        {
            _sessions[index] = session;
        }
        else
        {
            _sessions.Add(session);
        }

        return Task.CompletedTask;
    }

    public Task<List<CartItem>> GetItemsAsync(Guid sessionId) =>
        Task.FromResult(_items.Where(i => i.SessionId == sessionId).ToList());

    public Task SaveItemAsync(CartItem item)
    {
        var index = _items.FindIndex(i => i.Id == item.Id);
        if (index >= 0)
        {
            _items[index] = item;
        }
        else
        {
            _items.Add(item);
        }

        return Task.CompletedTask;
    }

    public Task DeleteItemAsync(Guid itemId)
    {
        _items.RemoveAll(i => i.Id == itemId);
        return Task.CompletedTask;
    }
}
