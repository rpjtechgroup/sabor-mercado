using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace SaborMercado.Modules.Rewards.Data;

/// <summary>
/// Design-time factory for EF migrations targeting PostgreSQL (production provider).
/// </summary>
public sealed class RewardsDbContextFactory : IDesignTimeDbContextFactory<RewardsDbContext>
{
    public RewardsDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<RewardsDbContext>();
        options.UseNpgsql(
            "Host=localhost;Database=sabormercado;Username=sabormercado;Password=changeme",
            npgsql => npgsql.MigrationsHistoryTable("__ef_migrations", "rewards"));

        return new RewardsDbContext(options.Options);
    }
}
