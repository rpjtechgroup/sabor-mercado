using SaborMercado.Web.Domain.Catalog;
using SaborMercado.Web.Domain.Shopping;
using SaborMercado.Web.Features.Catalog;
using SaborMercado.Web.Tests.Fakes;

namespace SaborMercado.Web.Tests.Features;

public class CatalogServiceTests
{
    private static readonly DateTimeOffset T0 = new(2026, 6, 11, 12, 0, 0, TimeSpan.Zero);

    private readonly InMemoryCatalogStore _store = new();
    private readonly InMemoryStoreStore _storeStore = new();
    private readonly FixedTimeProvider _clock = new(T0);

    public CatalogServiceTests()
    {
        CatalogTestStores.Seed(_storeStore, T0);
    }

    private async Task<CatalogService> CreateInitializedAsync()
    {
        var stores = new StoreService(_storeStore, _store, _clock);
        var service = new CatalogService(_store, stores, _clock);
        await service.InitializeAsync();
        return service;
    }

    private static Product NewProduct(string name = "Óleo de Soja") => new()
    {
        Name = name,
        Brand = "Liza",
        QuantityValue = 900m,
        QuantityUnit = QuantityUnit.Ml,
        Category = "Mercearia",
        StoreId = CatalogTestStores.DefaultId,
    };

    [Fact]
    public async Task CreateProduct_AddsToListAndPersists()
    {
        var service = await CreateInitializedAsync();

        var created = await service.CreateProductAsync(NewProduct());

        Assert.Single(service.Products);
        Assert.True(_store.Products.ContainsKey(created.Id));
        Assert.Equal(T0, created.CreatedAt);
    }

    [Fact]
    public async Task CreateProduct_WithoutName_Throws()
    {
        var service = await CreateInitializedAsync();

        await Assert.ThrowsAsync<ArgumentException>(
            () => service.CreateProductAsync(new Product { Name = "  ", StoreId = CatalogTestStores.DefaultId }));
    }

    [Fact]
    public async Task CreateProduct_WithoutStore_Throws()
    {
        var service = await CreateInitializedAsync();
        var product = NewProduct();
        product.StoreId = Guid.Empty;

        await Assert.ThrowsAsync<ArgumentException>(() => service.CreateProductAsync(product));
    }

    [Fact]
    public async Task UpdateProduct_ReflectsChangesImmediately()
    {
        var service = await CreateInitializedAsync();
        var product = await service.CreateProductAsync(NewProduct());

        product.Name = "Óleo de Soja Premium";
        await service.UpdateProductAsync(product);

        Assert.Equal("Óleo de Soja Premium", service.GetProduct(product.Id)?.Name);
        Assert.Equal("Óleo de Soja Premium", _store.Products[product.Id].Name);
    }

    [Fact]
    public async Task DeleteProduct_CascadesPriceRecords()
    {
        var service = await CreateInitializedAsync();
        var product = await service.CreateProductAsync(NewProduct());
        await service.AddPriceRecordAsync(product.Id, 8.99m, CatalogTestStores.StoreAId);
        await service.AddPriceRecordAsync(product.Id, 9.49m, CatalogTestStores.StoreBId);

        await service.DeleteProductAsync(product.Id);

        Assert.Empty(service.Products);
        Assert.Empty(_store.Products);
        Assert.Empty(_store.PriceRecords);
    }

    [Fact]
    public async Task PriceHistory_OrderedByObservedAtDescending()
    {
        var service = await CreateInitializedAsync();
        var product = await service.CreateProductAsync(NewProduct());

        await service.AddPriceRecordAsync(product.Id, 8.49m, CatalogTestStores.StoreAId, T0.AddDays(-10));
        await service.AddPriceRecordAsync(product.Id, 8.99m, CatalogTestStores.StoreBId, T0.AddDays(-1));
        await service.AddPriceRecordAsync(product.Id, 8.79m, CatalogTestStores.StoreCId, T0.AddDays(-5));

        var history = await service.GetPriceHistoryAsync(product.Id);

        Assert.Equal([8.99m, 8.79m, 8.49m], history.Select(r => r.Price).ToList());
    }

    [Fact]
    public async Task LastKnownPrice_IsMostRecentObservation()
    {
        var service = await CreateInitializedAsync();
        var product = await service.CreateProductAsync(NewProduct());
        await service.AddPriceRecordAsync(product.Id, 8.49m, CatalogTestStores.StoreAId, T0.AddDays(-10));
        await service.AddPriceRecordAsync(product.Id, 8.99m, CatalogTestStores.StoreAId, T0.AddDays(-1));

        var last = await service.GetLastKnownPriceAsync(product.Id);

        Assert.Equal(8.99m, last?.Price);
    }

    [Fact]
    public async Task AddPriceRecord_NegativePrice_Throws()
    {
        var service = await CreateInitializedAsync();
        var product = await service.CreateProductAsync(NewProduct());

        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(
            () => service.AddPriceRecordAsync(product.Id, -1m, CatalogTestStores.StoreAId));
    }

    [Fact]
    public async Task AddPriceRecord_UnknownProduct_Throws()
    {
        var service = await CreateInitializedAsync();

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.AddPriceRecordAsync(Guid.NewGuid(), 5m, CatalogTestStores.StoreAId));
    }

    [Fact]
    public async Task Initialize_RestoresPersistedProductsSortedByName()
    {
        var stores = new StoreService(_storeStore, _store, _clock);
        var seeded = new CatalogService(_store, stores, _clock);
        await seeded.InitializeAsync();
        await seeded.CreateProductAsync(NewProduct("Feijão"));
        await seeded.CreateProductAsync(NewProduct("Arroz"));

        var service = await CreateInitializedAsync();

        Assert.Equal(["Arroz", "Feijão"], service.Products.Select(p => p.Name).ToList());
    }

    [Fact]
    public async Task EnsureProduct_CreatesOnceForSameSnapshot()
    {
        var service = await CreateInitializedAsync();
        var snapshot = new ProductSnapshot("Leite Integral", "Itambé", 1m, QuantityUnit.L);

        var first = await service.EnsureProductAsync(snapshot);
        var second = await service.EnsureProductAsync(snapshot);

        Assert.Equal(first.Id, second.Id);
        Assert.Single(service.Products);
    }

    [Fact]
    public async Task SearchProducts_FiltersByPrefix()
    {
        var service = await CreateInitializedAsync();
        await service.CreateProductAsync(NewProduct("Arroz Branco"));
        await service.CreateProductAsync(NewProduct("Feijão Carioca"));

        var results = await service.SearchProductsAsync("arr");

        Assert.Single(results);
        Assert.Equal("Arroz Branco", results[0].Name);
    }

    [Fact]
    public async Task TouchPriceFromPurchase_SkipsDuplicateSameDay()
    {
        var service = await CreateInitializedAsync();
        var product = await service.CreateProductAsync(NewProduct());

        await service.TouchPriceFromPurchaseAsync(product.Id, 8.99m, CatalogTestStores.StoreAId, T0);
        await service.TouchPriceFromPurchaseAsync(product.Id, 8.99m, CatalogTestStores.StoreAId, T0.AddHours(2));

        var history = await service.GetPriceHistoryAsync(product.Id);
        Assert.Single(history);
    }
}
