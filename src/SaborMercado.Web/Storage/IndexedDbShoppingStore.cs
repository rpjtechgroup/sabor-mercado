using SaborMercado.Web.Domain.Shopping;
using SaborMercado.Web.Interop;

namespace SaborMercado.Web.Storage;

public sealed class IndexedDbShoppingStore(IndexedDbInterop indexedDb) : IShoppingStore
{
    public Task<List<ShoppingSession>> GetAllSessionsAsync() =>
        indexedDb.GetAllAsync<ShoppingSession>(StorageSchema.ShoppingSessionsStore).AsTask();

    public Task SaveSessionAsync(ShoppingSession session) =>
        indexedDb.PutAsync(StorageSchema.ShoppingSessionsStore, session).AsTask();

    public Task<List<CartItem>> GetItemsAsync(Guid sessionId) =>
        indexedDb.GetAllByIndexAsync<CartItem>(StorageSchema.CartItemsStore, "sessionId", sessionId).AsTask();

    public Task SaveItemAsync(CartItem item) =>
        indexedDb.PutAsync(StorageSchema.CartItemsStore, item).AsTask();

    public Task DeleteItemAsync(Guid itemId) =>
        indexedDb.DeleteAsync(StorageSchema.CartItemsStore, itemId).AsTask();
}
