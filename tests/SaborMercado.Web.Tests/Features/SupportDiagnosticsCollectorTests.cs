using System.Text.Json;
using Microsoft.AspNetCore.Components;
using SaborMercado.Web.Features.Account;
using SaborMercado.Web.Features.Catalog;
using SaborMercado.Web.Features.Shopping;
using SaborMercado.Web.Features.Support;
using SaborMercado.Web.Infrastructure;
using SaborMercado.Web.Interop;
using SaborMercado.Web.Shared;
using SaborMercado.Web.Tests.Fakes;

namespace SaborMercado.Web.Tests.Features;

public class SupportDiagnosticsCollectorTests
{
    [Fact]
    public async Task Collect_IncludesRouteAndStorageVersion()
    {
        var clock = new FixedTimeProvider(new DateTimeOffset(2026, 6, 16, 12, 0, 0, TimeSpan.Zero));
        var preferences = new InMemoryPreferencesStore();
        var api = new SaborMercadoApiClient(
            new HttpClient { BaseAddress = new Uri("http://localhost:9999") },
            preferences);
        var share = new ShareService(api, new InMemoryPendingShareStore(), clock);
        var account = new AccountService(api, preferences, share, clock);
        var catalogStore = new InMemoryCatalogStore();
        var storeStore = new InMemoryStoreStore();
        CatalogTestStores.Seed(storeStore, clock.GetUtcNow());
        var stores = new StoreService(storeStore, catalogStore, clock);
        var catalog = new CatalogService(catalogStore, stores, clock);
        var patterns = new ShoppingPatternService(new InMemoryShoppingPatternStore(), catalog, clock);
        var reminders = new ShoppingReminderService(new InMemoryShoppingReminderStore(), catalog, clock);
        var shopping = new ShoppingService(
            new InMemoryShoppingStore(),
            preferences,
            catalog,
            stores,
            patterns,
            reminders,
            new ToastService(clock),
            clock);

        var collector = new SupportDiagnosticsCollector(
            new TestNavigationManager(),
            account,
            shopping,
            catalogStore,
            storeStore,
            reminders,
            share,
            new StubSupportDiagnosticsInterop());

        var json = await collector.CollectAsync();

        Assert.Equal(JsonValueKind.Object, json.ValueKind);
        Assert.Contains("/compras", json.GetProperty("route").GetString());
        Assert.True(json.GetProperty("storage").GetProperty("indexedDbVersion").GetInt32() > 0);
    }

    private sealed class TestNavigationManager : NavigationManager
    {
        public TestNavigationManager() => Initialize("http://localhost/", "http://localhost/compras");
    }

    private sealed class StubSupportDiagnosticsInterop : ISupportDiagnosticsInterop
    {
        public ValueTask<bool> IsOnlineAsync() => ValueTask.FromResult(true);

        public ValueTask<ClientEnvironmentInfo> GetClientEnvironmentAsync() =>
            ValueTask.FromResult(new ClientEnvironmentInfo("agent", "pt-BR", 390, 844, true, true, false));
    }
}
