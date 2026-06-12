using SaborMercado.Web.Domain.Shopping;
using SaborMercado.Web.Interop;

namespace SaborMercado.Web.Storage;

public sealed class IndexedDbShoppingPatternStore(IndexedDbInterop indexedDb) : IShoppingPatternStore
{
    public async Task<ShoppingPattern?> GetAsync(Guid id) =>
        await indexedDb.GetAsync<ShoppingPattern>(StorageSchema.ShoppingPatternsStore, id);

    public async Task SaveAsync(ShoppingPattern pattern) =>
        await indexedDb.PutAsync(StorageSchema.ShoppingPatternsStore, pattern);
}
