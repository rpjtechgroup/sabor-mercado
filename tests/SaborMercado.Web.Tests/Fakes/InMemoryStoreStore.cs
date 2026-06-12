using SaborMercado.Web.Domain.Catalog;
using SaborMercado.Web.Storage;

namespace SaborMercado.Web.Tests.Fakes;

public sealed class InMemoryStoreStore : IStoreStore
{
    public Dictionary<Guid, Store> Stores { get; } = [];

    public Task<List<Store>> GetAllStoresAsync() =>
        Task.FromResult(Stores.Values.ToList());

    public Task SaveStoreAsync(Store store)
    {
        Stores[store.Id] = store;
        return Task.CompletedTask;
    }

    public Task DeleteStoreAsync(Guid storeId)
    {
        Stores.Remove(storeId);
        return Task.CompletedTask;
    }
}
