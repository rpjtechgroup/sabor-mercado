using SaborMercado.Web.Domain.Catalog;
using SaborMercado.Web.Interop;

namespace SaborMercado.Web.Storage;

public sealed class IndexedDbCatalogStore(IndexedDbInterop indexedDb) : ICatalogStore
{
    public Task<List<Product>> GetAllProductsAsync() =>
        indexedDb.GetAllAsync<Product>(StorageSchema.ProductsStore).AsTask();

    public Task SaveProductAsync(Product product) =>
        indexedDb.PutAsync(StorageSchema.ProductsStore, product).AsTask();

    public Task DeleteProductAsync(Guid productId) =>
        indexedDb.DeleteAsync(StorageSchema.ProductsStore, productId).AsTask();

    public Task<List<PriceRecord>> GetPriceRecordsAsync(Guid productId) =>
        indexedDb.GetAllByIndexAsync<PriceRecord>(StorageSchema.PriceRecordsStore, "productId", productId).AsTask();

    public Task SavePriceRecordAsync(PriceRecord record) =>
        indexedDb.PutAsync(StorageSchema.PriceRecordsStore, record).AsTask();

    public Task DeletePriceRecordsAsync(Guid productId) =>
        indexedDb.DeleteAllByIndexAsync(StorageSchema.PriceRecordsStore, "productId", productId).AsTask();
}
