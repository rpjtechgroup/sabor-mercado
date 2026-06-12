using SaborMercado.Web.Domain.Catalog;
using SaborMercado.Web.Interop;

namespace SaborMercado.Web.Storage;

public sealed class IndexedDbStoreStore(IndexedDbInterop indexedDb) : IStoreStore
{
    public Task<List<Store>> GetAllStoresAsync() =>
        indexedDb.GetAllAsync<Store>(StorageSchema.StoresStore).AsTask();

    public Task SaveStoreAsync(Store store) =>
        indexedDb.PutAsync(StorageSchema.StoresStore, store).AsTask();

    public Task DeleteStoreAsync(Guid storeId) =>
        indexedDb.DeleteAsync(StorageSchema.StoresStore, storeId).AsTask();
}
