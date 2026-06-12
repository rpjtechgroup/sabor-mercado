using SaborMercado.Web.Domain.Catalog;
using SaborMercado.Web.Features.Catalog;
using SaborMercado.Web.Features.Shopping;
using SaborMercado.Web.Storage;
using SaborMercado.Web.Tests.Fakes;

namespace SaborMercado.Web.Tests.Features;

public class ShoppingPatternServiceTests
{
    private static readonly DateTimeOffset Now = new(2026, 6, 11, 12, 0, 0, TimeSpan.Zero);

    private readonly InMemoryShoppingPatternStore _patternStore = new();
    private readonly InMemoryCatalogStore _catalogStore = new();
    private readonly InMemoryStoreStore _storeStore = new();
    private readonly FixedTimeProvider _clock = new(Now);

    public ShoppingPatternServiceTests()
    {
        CatalogTestStores.Seed(_storeStore, Now);
    }

    private ShoppingPatternService CreateService()
    {
        var stores = new StoreService(_storeStore, _catalogStore, _clock);
        var catalog = new CatalogService(_catalogStore, stores, _clock);
        return new ShoppingPatternService(_patternStore, catalog, _clock);
    }

    [Fact]
    public async Task GetOrCreateAsync_CreatesDefaultPattern()
    {
        var pattern = await CreateService().GetOrCreateAsync();

        Assert.Equal(StorageSchema.DefaultPatternId, pattern.Id);
        Assert.Empty(pattern.Items);
    }

    [Fact]
    public async Task AddAndRemoveProduct_UpdatesPattern()
    {
        var stores = new StoreService(_storeStore, _catalogStore, _clock);
        var catalog = new CatalogService(_catalogStore, stores, _clock);
        var product = await catalog.CreateProductAsync(new Product
        {
            Name = "Arroz",
            StoreId = CatalogTestStores.DefaultId,
        });
        var service = new ShoppingPatternService(_patternStore, catalog, _clock);

        await service.AddProductAsync(product.Id, defaultQuantity: 2);
        var lines = await service.GetLinesAsync();

        Assert.Single(lines);
        Assert.Equal(2, lines[0].DefaultQuantity);

        await service.RemoveProductAsync(product.Id);
        Assert.Empty(await service.GetLinesAsync());
    }
}
