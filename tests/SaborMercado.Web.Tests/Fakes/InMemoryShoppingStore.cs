using SaborMercado.Web.Domain.Shopping;
using SaborMercado.Web.Storage;

namespace SaborMercado.Web.Tests.Fakes;

public sealed class InMemoryShoppingStore : IShoppingStore
{
    public Dictionary<Guid, ShoppingSession> Sessions { get; } = [];

    public Dictionary<Guid, CartItem> Items { get; } = [];

    public bool FailOnWrite { get; set; }

    public Task<List<ShoppingSession>> GetAllSessionsAsync() =>
        Task.FromResult(Sessions.Values.ToList());

    public Task SaveSessionAsync(ShoppingSession session)
    {
        ThrowIfFailing();
        Sessions[session.Id] = session;
        return Task.CompletedTask;
    }

    public Task<List<CartItem>> GetItemsAsync(Guid sessionId) =>
        Task.FromResult(Items.Values.Where(i => i.SessionId == sessionId).ToList());

    public Task SaveItemAsync(CartItem item)
    {
        ThrowIfFailing();
        Items[item.Id] = item;
        return Task.CompletedTask;
    }

    public Task DeleteItemAsync(Guid itemId)
    {
        ThrowIfFailing();
        Items.Remove(itemId);
        return Task.CompletedTask;
    }

    private void ThrowIfFailing()
    {
        if (FailOnWrite)
        {
            throw new InvalidOperationException("Falha simulada de armazenamento.");
        }
    }
}
