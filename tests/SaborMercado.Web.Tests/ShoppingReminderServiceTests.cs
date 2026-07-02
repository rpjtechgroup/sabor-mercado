using SaborMercado.Web.Domain;
using SaborMercado.Web.Domain.Catalog;
using SaborMercado.Web.Domain.Gamification;
using SaborMercado.Web.Domain.Shopping;
using SaborMercado.Web.Features.Catalog;
using SaborMercado.Web.Features.Shopping;
using SaborMercado.Web.Storage;

namespace SaborMercado.Web.Tests;

public sealed class ShoppingReminderServiceTests
{
    [Fact]
    public async Task AddFromProductAsync_CreatesReminder_WithProductIdAndQuantity()
    {
        var product = CreateProduct("Arroz");
        var (service, _, _) = CreateService([product]);

        await service.AddFromProductAsync(product.Id, 2);

        var reminders = await service.GetAllAsync();
        var reminder = Assert.Single(reminders);
        Assert.Equal(product.Id, reminder.ProductId);
        Assert.Equal("Arroz", reminder.DisplayName);
        Assert.Equal(2, reminder.Quantity);
    }

    [Fact]
    public async Task AddFromProductAsync_DuplicateProduct_SumsQuantity()
    {
        var product = CreateProduct("Feijão");
        var (service, _, _) = CreateService([product]);

        await service.AddFromProductAsync(product.Id, 1);
        await service.AddFromProductAsync(product.Id, 3);

        var reminder = Assert.Single(await service.GetAllAsync());
        Assert.Equal(4, reminder.Quantity);
    }

    [Fact]
    public async Task AddFromProductAsync_UnknownProduct_Throws()
    {
        var (service, _, _) = CreateService([]);

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.AddFromProductAsync(Ids.NewId(), 1));

        Assert.Equal("Produto não encontrado no catálogo.", ex.Message);
    }

    [Fact]
    public async Task AddFromProductAsync_QuantityZero_Throws()
    {
        var product = CreateProduct("Leite");
        var (service, _, _) = CreateService([product]);

        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(
            () => service.AddFromProductAsync(product.Id, 0));
    }

    [Fact]
    public async Task GetLinesAsync_SkipsDeletedCatalogProduct()
    {
        var product = CreateProduct("Óleo");
        var (service, _, catalogService) = CreateService([product]);
        await service.AddFromProductAsync(product.Id, 1);

        await catalogService.DeleteProductAsync(product.Id);

        Assert.Empty(await service.GetLinesAsync());
    }

    [Fact]
    public async Task GetAllAsync_PurgesLegacyFreeTextReminders()
    {
        var product = CreateProduct("Açúcar");
        var (service, reminderStore, _) = CreateService([product]);
        var legacy = new ShoppingReminder
        {
            DisplayName = "papel higiênico",
            Quantity = 1,
            CreatedAt = DateTimeOffset.UtcNow,
        };
        await reminderStore.SaveAsync(legacy);
        await reminderStore.SaveAsync(new ShoppingReminder
        {
            ProductId = product.Id,
            DisplayName = product.Name,
            Quantity = 1,
            CreatedAt = DateTimeOffset.UtcNow,
        });

        var reminders = await service.GetAllAsync();

        var reminder = Assert.Single(reminders);
        Assert.Equal(product.Id, reminder.ProductId);
    }

    [Fact]
    public async Task ConsumeAllAsync_ClearsStore_RestoreAllAsync_RestoresItems()
    {
        var product = CreateProduct("Café");
        var (service, reminderStore, _) = CreateService([product]);
        await service.AddFromProductAsync(product.Id, 2);

        var consumed = await service.ConsumeAllAsync();

        Assert.Single(consumed);
        Assert.Empty(reminderStore.Reminders);
        Assert.Empty(await service.GetAllAsync());

        await service.RestoreAllAsync(consumed);

        var restored = Assert.Single(await service.GetAllAsync());
        Assert.Equal(product.Id, restored.ProductId);
        Assert.Equal(2, restored.Quantity);
    }

    [Fact]
    public async Task UpdateQuantityAsync_UpdatesAndThrowsWhenMissing()
    {
        var product = CreateProduct("Sal");
        var (service, _, _) = CreateService([product]);
        await service.AddFromProductAsync(product.Id, 1);
        var reminder = Assert.Single(await service.GetAllAsync());

        await service.UpdateQuantityAsync(reminder.Id, 5);

        Assert.Equal(5, (await service.GetAllAsync()).Single().Quantity);

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.UpdateQuantityAsync(Ids.NewId(), 2));

        Assert.Equal("Lembrete não encontrado.", ex.Message);

        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(
            () => service.UpdateQuantityAsync(reminder.Id, 0));
    }

    private static Product CreateProduct(string name)
    {
        return new Product
        {
            Id = Ids.NewId(),
            Name = name,
            StoreId = Guid.Empty,
            CreatedAt = DateTimeOffset.UtcNow,
        };
    }

    private static (ShoppingReminderService Service, FakeShoppingReminderStore ReminderStore, CatalogService CatalogService) CreateService(
        IEnumerable<Product> products)
    {
        var catalogStore = new InMemoryCatalogStore();
        catalogStore.Products.AddRange(products);
        var storeStore = new InMemoryStoreStore();
        var metrics = new NoOpMetricsStore();
        var clock = TimeProvider.System;
        var storeService = new StoreService(storeStore, catalogStore, metrics, clock);
        var catalogService = new CatalogService(catalogStore, storeService, metrics, clock);
        var reminderStore = new FakeShoppingReminderStore();
        var service = new ShoppingReminderService(reminderStore, catalogService, clock);
        return (service, reminderStore, catalogService);
    }

    private sealed class FakeShoppingReminderStore : IShoppingReminderStore
    {
        public List<ShoppingReminder> Reminders { get; } = [];

        public Task<List<ShoppingReminder>> GetAllAsync() => Task.FromResult(Reminders.ToList());

        public Task SaveAsync(ShoppingReminder reminder)
        {
            var index = Reminders.FindIndex(r => r.Id == reminder.Id);
            if (index >= 0)
            {
                Reminders[index] = reminder;
            }
            else
            {
                Reminders.Add(reminder);
            }

            return Task.CompletedTask;
        }

        public Task DeleteAsync(Guid reminderId)
        {
            Reminders.RemoveAll(r => r.Id == reminderId);
            return Task.CompletedTask;
        }

        public Task ClearAsync()
        {
            Reminders.Clear();
            return Task.CompletedTask;
        }
    }

    private sealed class InMemoryCatalogStore : ICatalogStore
    {
        public List<Product> Products { get; } = [];

        public Task<List<Product>> GetAllProductsAsync() => Task.FromResult(Products.ToList());

        public Task SaveProductAsync(Product product)
        {
            Products.Add(product);
            return Task.CompletedTask;
        }

        public Task DeleteProductAsync(Guid productId)
        {
            Products.RemoveAll(p => p.Id == productId);
            return Task.CompletedTask;
        }

        public Task<List<PriceRecord>> GetPriceRecordsAsync(Guid productId) =>
            Task.FromResult(new List<PriceRecord>());

        public Task SavePriceRecordAsync(PriceRecord record) => Task.CompletedTask;

        public Task DeletePriceRecordsAsync(Guid productId) => Task.CompletedTask;
    }

    private sealed class InMemoryStoreStore : IStoreStore
    {
        public Task<List<Store>> GetAllStoresAsync() => Task.FromResult(new List<Store>());

        public Task SaveStoreAsync(Store store) => Task.CompletedTask;

        public Task DeleteStoreAsync(Guid storeId) => Task.CompletedTask;
    }

    private sealed class NoOpMetricsStore : IMetricsStore
    {
        public Task<LocalGamificationMetrics> GetOrCreateMetricsAsync() =>
            Task.FromResult(new LocalGamificationMetrics());

        public Task SaveMetricsAsync(LocalGamificationMetrics metrics) => Task.CompletedTask;

        public Task IncrementProductCountAsync() => Task.CompletedTask;

        public Task IncrementStoreCountAsync() => Task.CompletedTask;

        public Task IncrementPurchaseCountAsync(bool budgetRespected) => Task.CompletedTask;

        public Task IncrementOcrCountAsync() => Task.CompletedTask;

        public Task SetProductsWithPriceHistoryCountAsync(int count) => Task.CompletedTask;

        public Task UpdateLoginStreakAsync(DateTimeOffset loginAt) => Task.CompletedTask;
    }
}
