using SaborMercado.Shared.StarterCatalog;
using SaborMercado.Web.Domain;
using SaborMercado.Web.Domain.Catalog;
using SaborMercado.Web.Domain.Gamification;
using SaborMercado.Web.Features.Catalog;
using SaborMercado.Web.Infrastructure;
using SaborMercado.Web.Interop;
using SaborMercado.Web.Shared;
using SaborMercado.Web.Domain.Shopping;
using SaborMercado.Web.Storage;

namespace SaborMercado.Web.Tests;

public sealed class StarterCatalogBootstrapServiceTests
{
    private static readonly string[] LegacyStoreKeys =
    [
        "carrefour",
        "atacadao",
        "pao-de-acucar",
        "assai",
        "extra",
        "sams-club",
    ];

    [Fact]
    public async Task ImportFromDto_WithLegacyStores_AddsOnlyNewStores()
    {
        var storeStore = new InMemoryStoreStore(LegacyStoreKeys);
        var catalogStore = new InMemoryCatalogStore();
        var service = CreateService(storeStore, catalogStore);
        var catalog = new StarterCatalogProvider().GetCatalog();

        var result = await service.ImportFromDtoAsync(catalog);

        Assert.Equal(catalog.Stores.Count - LegacyStoreKeys.Length, result.StoresAdded);
        Assert.Equal(329, result.ProductsAdded);
        Assert.Equal(catalog.Stores.Count, storeStore.Stores.Count);
    }

    [Fact]
    public async Task ImportFromDto_SecondImport_IsIdempotent()
    {
        var storeStore = new InMemoryStoreStore(LegacyStoreKeys);
        var catalogStore = new InMemoryCatalogStore();
        var service = CreateService(storeStore, catalogStore);
        var catalog = new StarterCatalogProvider().GetCatalog();

        await service.ImportFromDtoAsync(catalog);
        var second = await service.ImportFromDtoAsync(catalog);

        Assert.Equal(0, second.StoresAdded);
        Assert.Equal(0, second.ProductsAdded);
    }

    private static StarterCatalogBootstrapService CreateService(
        InMemoryStoreStore storeStore,
        InMemoryCatalogStore catalogStore)
    {
        var metrics = new NoOpMetricsStore();
        var clock = TimeProvider.System;
        var storeService = new StoreService(storeStore, catalogStore, metrics, clock);
        var catalogService = new CatalogService(catalogStore, storeService, metrics, clock);
        var apiClient = new SaborMercadoApiClient(new HttpClient(), new NoOpPreferencesStore());

        return new StarterCatalogBootstrapService(
            storeService,
            catalogService,
            storeStore,
            catalogStore,
            apiClient,
            new HttpClient(),
            new NoOpLocalStorageInterop(),
            new ToastService(clock),
            clock);
    }

    private sealed class InMemoryStoreStore : IStoreStore
    {
        public List<Store> Stores { get; } = [];

        public InMemoryStoreStore(IEnumerable<string> starterKeys)
        {
            var now = DateTimeOffset.UtcNow;
            foreach (var key in starterKeys)
            {
                Stores.Add(new Store
                {
                    Id = Ids.NewId(),
                    Name = key,
                    StarterKey = key,
                    CreatedAt = now,
                });
            }
        }

        public Task<List<Store>> GetAllStoresAsync() => Task.FromResult(Stores.ToList());

        public Task SaveStoreAsync(Store store)
        {
            Stores.Add(store);
            return Task.CompletedTask;
        }

        public Task DeleteStoreAsync(Guid storeId)
        {
            Stores.RemoveAll(s => s.Id == storeId);
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

    private sealed class NoOpPreferencesStore : IPreferencesStore
    {
        public Task<decimal?> GetBudgetDefaultAsync() => Task.FromResult<decimal?>(null);

        public Task SetBudgetDefaultAsync(decimal? value) => Task.CompletedTask;

        public Task<string?> GetGeminiApiKeyAsync() => Task.FromResult<string?>(null);

        public Task SetGeminiApiKeyAsync(string? value) => Task.CompletedTask;

        public Task<string?> GetAccessTokenAsync() => Task.FromResult<string?>(null);

        public Task<DateTimeOffset?> GetAccessTokenExpiresAtAsync() => Task.FromResult<DateTimeOffset?>(null);

        public Task<string?> GetAccountEmailAsync() => Task.FromResult<string?>(null);

        public Task<Guid?> GetPseudonymIdAsync() => Task.FromResult<Guid?>(null);

        public Task<string?> GetRefreshTokenAsync() => Task.FromResult<string?>(null);

        public Task SetAuthAsync(
            string accessToken,
            string refreshToken,
            DateTimeOffset expiresAt,
            string email,
            Guid pseudonymId) => Task.CompletedTask;

        public Task ClearAuthAsync() => Task.CompletedTask;

        public Task<bool> GetShowIconLabelsAsync() => Task.FromResult(false);

        public Task SetShowIconLabelsAsync(bool value) => Task.CompletedTask;

        public Task<IReadOnlyList<ComparatorColumnId>> GetComparatorColumnOrderAsync() =>
            Task.FromResult<IReadOnlyList<ComparatorColumnId>>(Array.Empty<ComparatorColumnId>());

        public Task SetComparatorColumnOrderAsync(IReadOnlyList<ComparatorColumnId> order) =>
            Task.CompletedTask;
    }

    private sealed class NoOpLocalStorageInterop : ILocalStorageInterop
    {
        public ValueTask SetItemAsync(string key, string value) => ValueTask.CompletedTask;

        public ValueTask<string?> GetItemAsync(string key) => ValueTask.FromResult<string?>(null);

        public ValueTask RemoveItemAsync(string key) => ValueTask.CompletedTask;
    }
}
