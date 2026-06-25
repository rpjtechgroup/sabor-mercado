using Microsoft.EntityFrameworkCore;
using SaborMercado.Modules.Rewards.Data;
using SaborMercado.Modules.Rewards.Services;
using SaborMercado.Shared.Rewards;

namespace SaborMercado.Modules.Rewards.Tests;

public sealed class RankingServiceTests
{
    [Fact]
    public async Task GetRankingAsync_OrdersByScoreDescending()
    {
        await using var db = CreateContext();
        var service = new RankingService(db, TimeProvider.System);
        var now = DateTimeOffset.UtcNow;

        db.UserGamificationMetrics.AddRange(
            new UserGamificationMetrics
            {
                Id = Guid.NewGuid(),
                UserId = Guid.Parse("11111111-1111-1111-1111-111111111111"),
                TotalProductsRegistered = 5,
                UpdatedAt = now,
            },
            new UserGamificationMetrics
            {
                Id = Guid.NewGuid(),
                UserId = Guid.Parse("22222222-2222-2222-2222-222222222222"),
                TotalProductsRegistered = 20,
                UpdatedAt = now,
            });

        await db.SaveChangesAsync();
        await service.RecalculateAllAsync(CancellationToken.None);

        var ranking = await service.GetRankingAsync(
            RankingTypes.Products,
            Guid.Parse("11111111-1111-1111-1111-111111111111"),
            CancellationToken.None);

        Assert.Equal(2, ranking.Entries.Count);
        Assert.Equal(20, ranking.Entries[0].Score);
        Assert.Equal(5, ranking.Entries[1].Score);
        Assert.Equal(2, ranking.CurrentUserRank);
        Assert.StartsWith("Usuario#", ranking.Entries[0].PseudonymDisplay, StringComparison.Ordinal);
    }

    [Fact]
    public async Task GetRankingAsync_UnknownType_ThrowsRewardsException()
    {
        await using var db = CreateContext();
        var service = new RankingService(db, TimeProvider.System);

        await Assert.ThrowsAsync<RewardsException>(() =>
            service.GetRankingAsync("invalid", null, CancellationToken.None));
    }

    private static RewardsDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<RewardsDbContext>()
            .UseSqlite("Data Source=:memory:")
            .Options;

        var db = new RewardsDbContext(options);
        db.Database.OpenConnection();
        db.Database.EnsureCreated();
        return db;
    }
}
