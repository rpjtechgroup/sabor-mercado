using Microsoft.EntityFrameworkCore;
using SaborMercado.Modules.Rewards.Data;
using SaborMercado.Modules.Rewards.Services;
using SaborMercado.Shared.Rewards;

namespace SaborMercado.Api.Tests.Rewards;

public class ContributionRewardServiceTests
{
    [Theory]
    [InlineData(false, false, 1)]
    [InlineData(false, true, 2)]
    [InlineData(true, false, 5)]
    [InlineData(true, true, 6)]
    public async Task GrantForAcceptedObservation_UsesCanonicalCreditRules(
        bool isNewProduct,
        bool hasValidEan,
        int expectedCredits)
    {
        await using var connection = new Microsoft.Data.Sqlite.SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();

        var contextOptions = new DbContextOptionsBuilder<RewardsDbContext>()
            .UseSqlite(connection)
            .Options;

        await using var db = new RewardsDbContext(contextOptions);
        await db.Database.EnsureCreatedAsync();

        var service = new ContributionRewardService(db, TimeProvider.System);
        var userId = Guid.NewGuid();
        var observationId = Guid.NewGuid();

        var granted = await service.GrantForAcceptedObservationAsync(
            userId,
            new ContributionRewardContext(observationId, isNewProduct, hasValidEan));

        Assert.Equal(expectedCredits, granted);
        var balance = await db.CreditLedgerEntries.Where(e => e.UserId == userId).SumAsync(e => e.Amount);
        Assert.Equal(expectedCredits, balance);
    }
}
