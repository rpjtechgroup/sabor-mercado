using SaborMercado.Web.Domain.Catalog;

namespace SaborMercado.Web.Storage;

public interface IStoreStore
{
    Task<List<Store>> GetAllStoresAsync();

    Task SaveStoreAsync(Store store);

    Task DeleteStoreAsync(Guid storeId);
}
