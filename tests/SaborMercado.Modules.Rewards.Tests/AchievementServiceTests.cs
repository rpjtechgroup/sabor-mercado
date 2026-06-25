using Microsoft.EntityFrameworkCore;
using SaborMercado.Modules.Rewards.Data;
using SaborMercado.Modules.Rewards.Services;
using SaborMercado.Shared.Community;

namespace SaborMercado.Modules.Rewards.Tests;

public sealed class AchievementServiceTests
{
    [Fact]
    public async Task EvaluateAfterMetricsSyncAsync_FirstProduct_UnlocksAchievement()
    {
        await using var db = CreateContext();
        var userId = Guid.NewGuid();
        var service = new AchievementService(db, TimeProvider.System);

        var response = await service.EvaluateAfterMetricsSyncAsync(
            userId,
            new UserMetricsValues(1, 0, 0, 0, 0, 0, 0));

        Assert.Contains(response.Items, item => item.Code == AchievementCodes.FirstProduct);
        var stored = await db.UserAchievements.SingleAsync();
        Assert.Equal(AchievementCodes.FirstProduct, stored.AchievementCode);
    }

    [Fact]
    public async Task EvaluateAfterMetricsSyncAsync_DoesNotDuplicateAchievement()
    {
        await using var db = CreateContext();
        var userId = Guid.NewGuid();
        var service = new AchievementService(db, TimeProvider.System);
        var metrics = new UserMetricsValues(10, 0, 0, 0, 0, 0, 0);

        await service.EvaluateAfterMetricsSyncAsync(userId, metrics);
        var second = await service.EvaluateAfterMetricsSyncAsync(userId, metrics);

        Assert.Empty(second.Items);
        Assert.Equal(2, await db.UserAchievements.CountAsync());
    }

    [Fact]
    public async Task EvaluateAfterMetricsSyncAsync_UnlocksMultipleAchievementsAtOnce()
    {
        await using var db = CreateContext();
        var userId = Guid.NewGuid();
        var service = new AchievementService(db, TimeProvider.System);

        var response = await service.EvaluateAfterMetricsSyncAsync(
            userId,
            new UserMetricsValues(10, 5, 10, 10, 10, 10, 7));

        Assert.Contains(response.Items, item => item.Code == AchievementCodes.FirstProduct);
        Assert.Contains(response.Items, item => item.Code == AchievementCodes.ProductCollector10);
        Assert.Contains(response.Items, item => item.Code == AchievementCodes.Shopper10);
        Assert.Contains(response.Items, item => item.Code == AchievementCodes.LoginStreak7);
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
