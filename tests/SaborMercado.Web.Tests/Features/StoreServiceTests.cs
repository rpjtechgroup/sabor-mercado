using SaborMercado.Web.Domain.Catalog;
using SaborMercado.Web.Features.Catalog;
using SaborMercado.Web.Tests.Fakes;

namespace SaborMercado.Web.Tests.Features;

public class StoreServiceTests
{
    private static readonly DateTimeOffset T0 = new(2026, 6, 11, 12, 0, 0, TimeSpan.Zero);

    private readonly InMemoryStoreStore _storeStore = new();
    private readonly InMemoryCatalogStore _catalogStore = new();
    private readonly FixedTimeProvider _clock = new(T0);

    private StoreService CreateService() => new(_storeStore, _catalogStore, _clock);

    [Fact]
    public async Task CreateStore_PersistsWithGeolocation()
    {
        var service = CreateService();
        await service.InitializeAsync();

        var created = await service.CreateStoreAsync(new Store
        {
            Name = "Mercado Central",
            City = "São Paulo",
            State = "SP",
            Latitude = -23.5505m,
            Longitude = -46.6333m,
        });

        Assert.Single(service.Stores);
        Assert.Equal(T0, created.CreatedAt);
        Assert.Equal(-23.5505m, created.Latitude);
    }

    [Fact]
    public async Task DeleteStore_WithLinkedProducts_Throws()
    {
        var stores = CreateService();
        await stores.InitializeAsync();
        var store = await stores.CreateStoreAsync(new Store { Name = "Mercado A" });

        var catalog = new CatalogService(_catalogStore, stores, _clock);
        await catalog.InitializeAsync();
        await catalog.CreateProductAsync(new Product { Name = "Arroz", StoreId = store.Id });

        await Assert.ThrowsAsync<InvalidOperationException>(() => stores.DeleteStoreAsync(store.Id));
    }
}
