using SaborMercado.Web.Domain.Catalog;
using SaborMercado.Web.Features.Catalog;
using SaborMercado.Web.Features.Shopping;
using SaborMercado.Web.Tests.Fakes;

namespace SaborMercado.Web.Tests.Features;

public class ShoppingReminderServiceTests
{
    private static readonly DateTimeOffset T0 = new(2026, 6, 16, 12, 0, 0, TimeSpan.Zero);

    private readonly InMemoryShoppingReminderStore _reminderStore = new();
    private readonly InMemoryCatalogStore _catalogStore = new();
    private readonly InMemoryStoreStore _storeStore = new();
    private readonly FixedTimeProvider _clock = new(T0);

    public ShoppingReminderServiceTests()
    {
        CatalogTestStores.Seed(_storeStore, T0);
    }

    private ShoppingReminderService CreateService()
    {
        var stores = new StoreService(_storeStore, _catalogStore, _clock);
        var catalog = new CatalogService(_catalogStore, stores, _clock);
        return new ShoppingReminderService(_reminderStore, catalog, _clock);
    }

    [Fact]
    public async Task AddFromProductAsync_DuplicateProduct_MergesQuantity()
    {
        var stores = new StoreService(_storeStore, _catalogStore, _clock);
        var catalog = new CatalogService(_catalogStore, stores, _clock);
        var product = await catalog.CreateProductAsync(new Product
        {
            Name = "Leite",
            StoreId = CatalogTestStores.StoreAId,
        });

        var service = CreateService();
        await service.AddFromProductAsync(product.Id, quantity: 1);
        await service.AddFromProductAsync(product.Id, quantity: 2);

        var reminders = await service.GetAllAsync();
        var reminder = Assert.Single(reminders);
        Assert.Equal(3, reminder.Quantity);
        Assert.Equal(product.Id, reminder.ProductId);
    }

    [Fact]
    public async Task AddFromNoteAsync_DuplicateName_MergesQuantity()
    {
        var service = CreateService();
        await service.AddFromNoteAsync("Papel higiênico", quantity: 1);
        await service.AddFromNoteAsync("PAPEL HIGIÊNICO", quantity: 2);

        var reminder = Assert.Single(await service.GetAllAsync());
        Assert.Equal(3, reminder.Quantity);
        Assert.Null(reminder.ProductId);
    }

    [Fact]
    public async Task ConsumeAllAsync_ClearsStore()
    {
        var service = CreateService();
        await service.AddFromNoteAsync("Café", quantity: 1);

        var consumed = await service.ConsumeAllAsync();

        Assert.Single(consumed);
        Assert.Empty(await service.GetAllAsync());
    }

    [Fact]
    public async Task RestoreAllAsync_RepopulatesStore()
    {
        var service = CreateService();
        await service.AddFromNoteAsync("Café", quantity: 1);
        var consumed = await service.ConsumeAllAsync();

        await service.RestoreAllAsync(consumed);

        Assert.Single(await service.GetAllAsync());
    }
}
