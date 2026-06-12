using SaborMercado.Web.Domain.Catalog;
using SaborMercado.Web.Domain.Shopping;
using SaborMercado.Web.Domain.Status;
using SaborMercado.Web.Features.Catalog;
using SaborMercado.Web.Features.Shopping;
using SaborMercado.Web.Storage;
using SaborMercado.Web.Tests.Fakes;

namespace SaborMercado.Web.Tests.Features;

public class ShoppingServiceTests
{
    private static readonly DateTimeOffset T0 = new(2026, 6, 11, 12, 0, 0, TimeSpan.Zero);

    private readonly InMemoryShoppingStore _store = new();
    private readonly InMemoryCatalogStore _catalogStore = new();
    private readonly InMemoryShoppingPatternStore _patternStore = new();
    private readonly InMemoryPreferencesStore _preferences = new();
    private readonly FixedTimeProvider _clock = new(T0);

    private ShoppingService CreateService()
    {
        var catalog = new CatalogService(_catalogStore, _clock);
        var patterns = new ShoppingPatternService(_patternStore, catalog, _clock);
        return new ShoppingService(_store, _preferences, catalog, patterns, _clock);
    }

    private static ProductSnapshot Snapshot(string name = "Óleo de Soja Liza") =>
        new(name, "Liza", 900m, QuantityUnit.Ml);

    private static async Task<ShoppingService> StartedAsync(ShoppingService service, decimal? budget = 100m)
    {
        await service.InitializeAsync();
        await service.StartSessionAsync(budget, "Mercado Teste");
        return service;
    }

    // ----- Sessão -----

    [Fact]
    public async Task StartSession_CreatesActiveSessionAndEmitsBudgetSet()
    {
        var service = await StartedAsync(CreateService(), budget: 300m);

        Assert.Equal(SessionStatus.Active, service.CurrentSession?.Status);
        Assert.Equal(300m, service.CurrentSession?.BudgetAmount);
        Assert.Equal(StatusCodes.BudgetSet, service.LastMessage?.Code);
        Assert.Single(_store.Sessions);
    }

    [Fact]
    public async Task StartSession_WithActiveSession_Throws()
    {
        var service = await StartedAsync(CreateService());

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.StartSessionAsync(50m, null));
    }

    [Fact]
    public async Task StartSession_PersistsBudgetAsPreference()
    {
        await StartedAsync(CreateService(), budget: 250m);

        Assert.Equal(250m, _preferences.BudgetDefault);
    }

    [Fact]
    public async Task Initialize_RestoresActiveSessionAndItems()
    {
        var first = await StartedAsync(CreateService());
        await first.AddItemAsync(Snapshot(), 8.99m, 3, CartItemSource.Manual);

        var restored = CreateService();
        await restored.InitializeAsync();

        Assert.NotNull(restored.CurrentSession);
        Assert.Single(restored.Items);
        Assert.Equal(26.97m, restored.Total);
    }

    // ----- Carrinho (F2) -----

    [Fact]
    public async Task AddItem_UpdatesTotalAndPersistsImmediately()
    {
        var service = await StartedAsync(CreateService());

        await service.AddItemAsync(Snapshot(), 8.99m, 1, CartItemSource.Manual);

        Assert.Equal(8.99m, service.Total);
        Assert.Single(_store.Items);
        Assert.Equal(CartItemSource.Manual, service.Items[0].Source);
    }

    [Fact]
    public async Task AddItem_CreatesCatalogProductAndSetsProductId()
    {
        var service = await StartedAsync(CreateService());

        await service.AddItemAsync(Snapshot(), 8.99m, 1, CartItemSource.Manual);

        Assert.NotNull(service.Items[0].ProductId);
        Assert.Single(_catalogStore.Products);
        Assert.NotEmpty(_catalogStore.PriceRecords);
    }

    [Fact]
    public async Task IncrementQuantity_PlusOneAndPlusFive_RecalculatesSubtotal()
    {
        var service = await StartedAsync(CreateService());
        await service.AddItemAsync(Snapshot(), 8.99m, 1, CartItemSource.Manual);
        var itemId = service.Items[0].Id;

        await service.IncrementQuantityAsync(itemId, 1);
        await service.IncrementQuantityAsync(itemId, 1);
        Assert.Equal(26.97m, service.Total); // 3 × R$ 8,99

        await service.IncrementQuantityAsync(itemId, 5);
        Assert.Equal(8, service.Items[0].Quantity);
    }

    [Fact]
    public async Task SetQuantity_TypedValue_Recalculates()
    {
        var service = await StartedAsync(CreateService());
        await service.AddItemAsync(Snapshot(), 2m, 1, CartItemSource.Manual);

        await service.SetQuantityAsync(service.Items[0].Id, 12);

        Assert.Equal(24m, service.Total);
    }

    [Fact]
    public async Task SetQuantity_BelowOne_Throws()
    {
        var service = await StartedAsync(CreateService());
        await service.AddItemAsync(Snapshot(), 2m, 1, CartItemSource.Manual);

        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(
            () => service.SetQuantityAsync(service.Items[0].Id, 0));
    }

    [Fact]
    public async Task AddItem_NegativePrice_Throws()
    {
        var service = await StartedAsync(CreateService());

        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(
            () => service.AddItemAsync(Snapshot(), -1m, 1, CartItemSource.Manual));
    }

    [Fact]
    public async Task AddItem_ZeroPrice_IsAllowed()
    {
        var service = await StartedAsync(CreateService());

        await service.AddItemAsync(Snapshot("Brinde"), 0m, 1, CartItemSource.Manual);

        Assert.Equal(0m, service.Total);
    }

    [Fact]
    public async Task RemoveItem_RecalculatesAndDeletesFromStore()
    {
        var service = await StartedAsync(CreateService());
        await service.AddItemAsync(Snapshot(), 8.99m, 1, CartItemSource.Manual);

        await service.RemoveItemAsync(service.Items[0].Id);

        Assert.Empty(service.Items);
        Assert.Empty(_store.Items);
        Assert.Equal(0m, service.Total);
    }

    [Fact]
    public async Task UpdateItem_ChangesSnapshotPriceAndQuantity()
    {
        var service = await StartedAsync(CreateService());
        await service.AddItemAsync(Snapshot(), 8.99m, 1, CartItemSource.Manual);

        await service.UpdateItemAsync(service.Items[0].Id, Snapshot("Arroz"), 20m, 2);

        Assert.Equal("Arroz", service.Items[0].ProductSnapshot.Name);
        Assert.Equal(40m, service.Total);
    }

    // ----- Alertas integrados (F4) -----

    [Fact]
    public async Task BudgetAlert_Crosses50Percent_EmitsBudgetHalf()
    {
        var service = await StartedAsync(CreateService(), budget: 100m);

        await service.AddItemAsync(Snapshot(), 55m, 1, CartItemSource.Manual);

        Assert.Equal(StatusCodes.BudgetHalf, service.LastMessage?.Code);
    }

    [Fact]
    public async Task BudgetAlert_PersistsAlertStateWithSession()
    {
        var service = await StartedAsync(CreateService(), budget: 100m);
        await service.AddItemAsync(Snapshot(), 55m, 1, CartItemSource.Manual);

        var persisted = _store.Sessions.Values.Single();

        Assert.Contains(StatusCodes.BudgetHalf, persisted.AlertState.EmittedCodes);
        Assert.Equal(55m, persisted.AlertState.LastPercentUsed);
    }

    [Fact]
    public async Task BudgetAlert_RearmAfterRemoval_EmitsAgainOnRecross()
    {
        var service = await StartedAsync(CreateService(), budget: 100m);
        await service.AddItemAsync(Snapshot("A"), 55m, 1, CartItemSource.Manual);
        await service.AddItemAsync(Snapshot("B"), 10m, 1, CartItemSource.Manual);

        await service.RemoveItemAsync(service.Items[0].Id); // P volta a 10%
        await service.AddItemAsync(Snapshot("C"), 45m, 1, CartItemSource.Manual); // cruza 50% de novo

        Assert.Equal(StatusCodes.BudgetHalf, service.LastMessage?.Code);
    }

    // ----- Encerramento -----

    [Fact]
    public async Task Finish_SetsStatusAndEmitsSessionFinished()
    {
        var service = await StartedAsync(CreateService(), budget: 100m);
        await service.AddItemAsync(Snapshot(), 80m, 1, CartItemSource.Manual);

        await service.FinishSessionAsync();

        Assert.Null(service.CurrentSession);
        Assert.Equal(StatusCodes.SessionFinished, service.LastMessage?.Code);
        Assert.Equal(SessionStatus.Finished, _store.Sessions.Values.Single().Status);
        Assert.Equal(T0, _store.Sessions.Values.Single().FinishedAt);
    }

    [Fact]
    public async Task Abandon_SetsStatusAbandoned_WithoutNewMessage()
    {
        var service = await StartedAsync(CreateService(), budget: null);
        var versionBefore = service.MessageVersion;

        await service.AbandonSessionAsync();

        Assert.Null(service.CurrentSession);
        Assert.Equal(SessionStatus.Abandoned, _store.Sessions.Values.Single().Status);
        Assert.Equal(versionBefore, service.MessageVersion);
    }

    // ----- Compra mensal -----

    [Fact]
    public async Task StartMonthlySession_PrefillsCartWithoutBudget()
    {
        var catalog = new CatalogService(_catalogStore, _clock);
        var patterns = new ShoppingPatternService(_patternStore, catalog, _clock);
        var product = await catalog.CreateProductAsync(new Product { Name = "Feijão", QuantityValue = 1m, QuantityUnit = QuantityUnit.Kg });
        await catalog.AddPriceRecordAsync(product.Id, 8.50m, "Mercado A");
        await patterns.AddProductAsync(product.Id, defaultQuantity: 2);

        var service = new ShoppingService(_store, _preferences, catalog, patterns, _clock);
        await service.InitializeAsync();
        await service.StartSessionAsync(SessionKind.Monthly, null, "Mercado Mensal");

        Assert.Equal(SessionKind.Monthly, service.CurrentSession?.Kind);
        Assert.Null(service.CurrentSession?.BudgetAmount);
        Assert.Null(service.LastMessage);
        var item = Assert.Single(service.Items);
        Assert.Equal(2, item.Quantity);
        Assert.Equal(8.50m, item.UnitPrice);
    }

    [Fact]
    public async Task StartSporadicSession_StillSupportsBudget()
    {
        var service = await StartedAsync(CreateService(), budget: 150m);

        Assert.Equal(SessionKind.Sporadic, service.CurrentSession?.Kind);
        Assert.Equal(150m, service.CurrentSession?.BudgetAmount);
        Assert.Equal(StatusCodes.BudgetSet, service.LastMessage?.Code);
    }

    // ----- Falha de armazenamento (edge case da spec) -----

    [Fact]
    public async Task StorageFailure_KeepsFlowInMemoryAndFlagsUnavailable()
    {
        var service = await StartedAsync(CreateService());
        _store.FailOnWrite = true;

        await service.AddItemAsync(Snapshot(), 8.99m, 2, CartItemSource.Manual);

        Assert.True(service.StorageUnavailable);
        Assert.Equal(17.98m, service.Total); // fluxo continua em memória
    }
}
