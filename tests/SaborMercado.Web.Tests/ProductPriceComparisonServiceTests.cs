using SaborMercado.Web.Domain.Catalog;
using SaborMercado.Web.Domain.Shopping;
using SaborMercado.Web.Features.Shopping;

namespace SaborMercado.Web.Tests;

public sealed class ProductPriceComparisonServiceTests
{
    private static readonly DateTimeOffset Now = new(2026, 7, 1, 12, 0, 0, TimeSpan.Zero);

    [Fact]
    public async Task EmptyStore_ReturnsEmpty()
    {
        var service = CreateService(new FakeShoppingStore(), Now);
        var rows = await service.GetComparisonRowsAsync();
        Assert.Empty(rows);
    }

    [Fact]
    public async Task SessionOutsideWindow_Excluded()
    {
        var store = new FakeShoppingStore();
        var session = FinishedSession(Now.AddDays(-91), "Mercado A");
        store.AddSession(session);
        store.AddItem(CreateItem(session.Id, "Arroz", 5.99m, "Mercado A"));

        var rows = await CreateService(store, Now).GetComparisonRowsAsync();
        Assert.Empty(rows);
    }

    [Fact]
    public async Task SingleObservation_BestEqualsWorst()
    {
        var store = new FakeShoppingStore();
        var session = FinishedSession(Now.AddDays(-1), "Mercado A");
        store.AddSession(session);
        store.AddItem(CreateItem(session.Id, "Arroz", 5.99m, "Mercado A"));

        var row = Assert.Single(await CreateService(store, Now).GetComparisonRowsAsync());
        Assert.Equal(5.99m, row.BestPrice);
        Assert.Equal(5.99m, row.WorstPrice);
    }

    [Fact]
    public async Task MultipleMarkets_PicksCheapestStore()
    {
        var store = new FakeShoppingStore();
        var sessionA = FinishedSession(Now.AddDays(-10), "Mercado A");
        var sessionB = FinishedSession(Now.AddDays(-5), "Mercado B");
        store.AddSession(sessionA);
        store.AddSession(sessionB);
        store.AddItem(CreateItem(sessionA.Id, "Arroz", 6.50m, "Mercado A"));
        store.AddItem(CreateItem(sessionB.Id, "Arroz", 4.20m, "Mercado B"));

        var row = Assert.Single(await CreateService(store, Now).GetComparisonRowsAsync());
        Assert.Equal("Mercado B", row.BestMarketName);
        Assert.Equal(4.20m, row.BestPrice);
        Assert.Equal(6.50m, row.WorstPrice);
    }

    [Fact]
    public async Task TieOnBestPrice_PicksMostRecentDate()
    {
        var store = new FakeShoppingStore();
        var older = FinishedSession(Now.AddDays(-20), "Mercado A");
        var newer = FinishedSession(Now.AddDays(-2), "Mercado B");
        store.AddSession(older);
        store.AddSession(newer);
        store.AddItem(CreateItem(older.Id, "Feijão", 3.00m, "Mercado A"));
        store.AddItem(CreateItem(newer.Id, "Feijão", 3.00m, "Mercado B"));

        var row = Assert.Single(await CreateService(store, Now).GetComparisonRowsAsync());
        Assert.Equal("Mercado B", row.BestMarketName);
        Assert.Equal(DateOnly.FromDateTime(newer.FinishedAt!.Value.UtcDateTime), row.BestPriceDate);
    }

    [Fact]
    public async Task BestAndWorst_FromAllObservations()
    {
        var store = new FakeShoppingStore();
        var session = FinishedSession(Now.AddDays(-3), "Mercado Central");
        store.AddSession(session);
        store.AddItem(CreateItem(session.Id, "Leite", 4.00m, "Mercado Central"));
        store.AddItem(CreateItem(session.Id, "Leite", 7.50m, "Mercado Central"));
        store.AddItem(CreateItem(session.Id, "Leite", 5.25m, "Mercado Central"));

        var row = Assert.Single(await CreateService(store, Now).GetComparisonRowsAsync());
        Assert.Equal(4.00m, row.BestPrice);
        Assert.Equal(7.50m, row.WorstPrice);
    }

    [Fact]
    public async Task ProductsOrderedAlphabetically()
    {
        var store = new FakeShoppingStore();
        var session = FinishedSession(Now.AddDays(-1), "Mercado A");
        store.AddSession(session);
        store.AddItem(CreateItem(session.Id, "Banana", 2.00m, "Mercado A"));
        store.AddItem(CreateItem(session.Id, "Abacaxi", 5.00m, "Mercado A"));

        var rows = await CreateService(store, Now).GetComparisonRowsAsync();
        Assert.Equal(["Abacaxi", "Banana"], rows.Select(r => r.ProductName).ToArray());
    }

    private static ProductPriceComparisonService CreateService(FakeShoppingStore store, DateTimeOffset utcNow) =>
        new(store, new FixedTimeProvider(utcNow));

    private static ShoppingSession FinishedSession(DateTimeOffset finishedAt, string marketName) =>
        new()
        {
            MarketName = marketName,
            StartedAt = finishedAt.AddHours(-1),
            FinishedAt = finishedAt,
            Status = SessionStatus.Finished
        };

    private static CartItem CreateItem(Guid sessionId, string name, decimal price, string storeName) =>
        new()
        {
            SessionId = sessionId,
            ProductSnapshot = new ProductSnapshot(name, null, null, null),
            UnitPrice = price,
            StoreName = storeName,
            AddedAt = DateTimeOffset.UtcNow
        };

    private sealed class FixedTimeProvider(DateTimeOffset utcNow) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => utcNow;
    }
}
