using SaborMercado.Web.Domain.Catalog;
using SaborMercado.Web.Domain.Shopping;
using SaborMercado.Web.Features.Shopping;
using SaborMercado.Web.Tests.Fakes;

namespace SaborMercado.Web.Tests.Features;

public class PurchaseHistoryServiceTests
{
    private static readonly DateTimeOffset Now = new(2026, 6, 11, 12, 0, 0, TimeSpan.Zero);

    private readonly InMemoryShoppingStore _store = new();
    private readonly FixedTimeProvider _clock = new(Now);

    private PurchaseHistoryService CreateService() => new(_store, _clock);

    private static ProductSnapshot Milk => new("Leite", "Itambé", 1m, QuantityUnit.L);

    [Fact]
    public async Task GetRecentSessions_ExcludesOlderThanThreeMonths()
    {
        var recent = FinishedSession(Now.AddDays(-10), "Mercado A");
        var old = FinishedSession(Now.AddDays(-120), "Mercado B");
        _store.Sessions[recent.Id] = recent;
        _store.Sessions[old.Id] = old;

        var service = CreateService();
        var sessions = await service.GetRecentSessionsAsync();

        Assert.Single(sessions);
        Assert.Equal(recent.Id, sessions[0].SessionId);
    }

    [Fact]
    public async Task GetSessionGrid_OrdersAlphabeticallyAndIncludesMarket()
    {
        var session = FinishedSession(Now.AddDays(-1), "Mercado Central");
        _store.Sessions[session.Id] = session;
        AddItem(session.Id, new ProductSnapshot("Feijão", null, null, null), 6m, 1);
        AddItem(session.Id, new ProductSnapshot("Arroz", null, null, null), 5m, 2);

        var rows = await CreateService().GetSessionGridAsync(session.Id);

        Assert.Equal(["Arroz", "Feijão"], rows.Select(r => r.ProductName).ToList());
        Assert.All(rows, r => Assert.Equal("Mercado Central", r.MarketName));
    }

    [Fact]
    public async Task GetConsolidatedGrid_AssignsPriceTrends()
    {
        var firstSession = FinishedSession(Now.AddDays(-20), "Mercado A");
        var secondSession = FinishedSession(Now.AddDays(-5), "Mercado B");
        _store.Sessions[firstSession.Id] = firstSession;
        _store.Sessions[secondSession.Id] = secondSession;

        AddItem(firstSession.Id, Milk, 5.00m, 1);
        AddItem(secondSession.Id, Milk, 4.50m, 1);

        var rows = await CreateService().GetConsolidatedGridAsync();
        var milkRows = rows.Where(r => r.ProductName == "Leite").OrderBy(r => r.PurchaseDate).ToList();

        Assert.Equal(PriceTrend.None, milkRows[0].Trend);
        Assert.Equal(PriceTrend.Cheaper, milkRows[1].Trend);
    }

    [Fact]
    public async Task GetConsolidatedGrid_MarksMoreExpensiveInRedTrend()
    {
        var firstSession = FinishedSession(Now.AddDays(-20), "Mercado A");
        var secondSession = FinishedSession(Now.AddDays(-5), "Mercado B");
        _store.Sessions[firstSession.Id] = firstSession;
        _store.Sessions[secondSession.Id] = secondSession;

        AddItem(firstSession.Id, Milk, 4.50m, 1);
        AddItem(secondSession.Id, Milk, 5.20m, 1);

        var rows = await CreateService().GetConsolidatedGridAsync();
        var latest = rows.Single(r => r.PurchaseDate == DateOnly.FromDateTime(secondSession.FinishedAt!.Value.UtcDateTime));

        Assert.Equal(PriceTrend.MoreExpensive, latest.Trend);
    }

    [Fact]
    public async Task GetConsolidatedGrid_ComputesDeltaVsPrevious()
    {
        var firstSession = FinishedSession(Now.AddDays(-20), "Mercado A");
        var secondSession = FinishedSession(Now.AddDays(-5), "Mercado B");
        _store.Sessions[firstSession.Id] = firstSession;
        _store.Sessions[secondSession.Id] = secondSession;

        AddItem(firstSession.Id, Milk, 5.00m, 1);
        AddItem(secondSession.Id, Milk, 4.50m, 1);

        var rows = await CreateService().GetConsolidatedGridAsync();
        var latest = rows.Single(r => r.PurchaseDate == DateOnly.FromDateTime(secondSession.FinishedAt!.Value.UtcDateTime));

        Assert.Equal(-0.50m, latest.DeltaVsPrevious);
    }

    [Fact]
    public async Task GetConsolidatedGrid_FirstPurchaseHasNullDelta()
    {
        var session = FinishedSession(Now.AddDays(-5), "Mercado A");
        _store.Sessions[session.Id] = session;
        AddItem(session.Id, Milk, 4.50m, 1);

        var row = Assert.Single(await CreateService().GetConsolidatedGridAsync());

        Assert.Null(row.DeltaVsPrevious);
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

    private void AddItem(Guid sessionId, ProductSnapshot snapshot, decimal price, int quantity)
    {
        var item = new CartItem
        {
            Id = Guid.NewGuid(),
            SessionId = sessionId,
            ProductSnapshot = snapshot,
            UnitPrice = price,
            Quantity = quantity,
            AddedAt = Now,
        };
        _store.Items[item.Id] = item;
    }
}
