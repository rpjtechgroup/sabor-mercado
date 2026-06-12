using SaborMercado.Web.Domain.Catalog;

namespace SaborMercado.Web.Storage;

public interface ICatalogStore
{
    Task<List<Product>> GetAllProductsAsync();

    Task SaveProductAsync(Product product);

    Task DeleteProductAsync(Guid productId);

    Task<List<PriceRecord>> GetPriceRecordsAsync(Guid productId);

    Task SavePriceRecordAsync(PriceRecord record);

    Task DeletePriceRecordsAsync(Guid productId);
}
