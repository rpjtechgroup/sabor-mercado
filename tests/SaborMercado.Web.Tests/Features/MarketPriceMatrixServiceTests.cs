using SaborMercado.Web.Domain.Catalog;
using SaborMercado.Web.Domain.Shopping;
using SaborMercado.Web.Features.Shopping;
using SaborMercado.Web.Tests.Fakes;

namespace SaborMercado.Web.Tests.Features;

public class MarketPriceMatrixServiceTests
{
    private static readonly DateTimeOffset Now = new(2026, 6, 11, 12, 0, 0, TimeSpan.Zero);

    private readonly InMemoryShoppingStore _store = new();
    private readonly FixedTimeProvider _clock = new(Now);

    private MarketPriceMatrixService CreateService() => new(_store, _clock);

    private static ProductSnapshot Oil => new("Óleo", "Liza", 900m, QuantityUnit.Ml);

    [Fact]
    public async Task GetMatrix_KeepsLatestPricePerMarket()
    {
        var older = FinishedSession(Now.AddDays(-30), "Mercado A");
        var newer = FinishedSession(Now.AddDays(-5), "Mercado A");
        _store.Sessions[older.Id] = older;
        _store.Sessions[newer.Id] = newer;

        AddItem(older.Id, Oil, 8.00m);
        AddItem(newer.Id, Oil, 7.50m);

        var rows = await CreateService().GetMatrixAsync();
        var oil = Assert.Single(rows);

        Assert.Equal(7.50m, Assert.Single(oil.Cells).UnitPrice);
        Assert.Equal("Mercado A", oil.BestMarketName);
    }

    [Fact]
    public async Task GetMatrix_HighlightsCheapestMarket()
    {
        var sessionA = FinishedSession(Now.AddDays(-10), "Mercado A");
        var sessionB = FinishedSession(Now.AddDays(-8), "Mercado B");
        _store.Sessions[sessionA.Id] = sessionA;
        _store.Sessions[sessionB.Id] = sessionB;

        AddItem(sessionA.Id, Oil, 9.00m);
        AddItem(sessionB.Id, Oil, 7.20m);

        var row = Assert.Single(await CreateService().GetMatrixAsync());

        Assert.Equal("Mercado B", row.BestMarketName);
        Assert.Equal(7.20m, row.BestPrice);
        Assert.Equal(2, row.Cells.Count);
    }

    [Fact]
    public async Task GetMatrix_ExcludesSessionsOlderThanThreeMonths()
    {
        var recent = FinishedSession(Now.AddDays(-20), "Mercado A");
        var old = FinishedSession(Now.AddDays(-120), "Mercado Velho");
        _store.Sessions[recent.Id] = recent;
        _store.Sessions[old.Id] = old;

        AddItem(recent.Id, Oil, 5m);
        AddItem(old.Id, Oil, 3m);

        var row = Assert.Single(await CreateService().GetMatrixAsync());

        Assert.Single(row.Cells);
        Assert.Equal("Mercado A", row.Cells[0].MarketName);
    }

    private ShoppingSession FinishedSession(DateTimeOffset finishedAt, string market) =>
        new()
        {
            Id = Guid.NewGuid(),
            MarketName = market,
            StartedAt = finishedAt.AddHours(-1),
            FinishedAt = finishedAt,
            Status = SessionStatus.Finished,
        };

    private void AddItem(Guid sessionId, ProductSnapshot snapshot, decimal price)
    {
        var item = new CartItem
        {
            Id = Guid.NewGuid(),
            SessionId = sessionId,
            ProductSnapshot = snapshot,
            UnitPrice = price,
            Quantity = 1,
            AddedAt = Now,
        };
        _store.Items[item.Id] = item;
    }
}
