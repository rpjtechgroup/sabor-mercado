using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace SaborMercado.Modules.Identity.Data;

/// <summary>
/// Design-time factory for EF migrations targeting PostgreSQL (production provider).
/// </summary>
public sealed class IdentityDbContextFactory : IDesignTimeDbContextFactory<IdentityDbContext>
{
    public IdentityDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<IdentityDbContext>();
        options.UseNpgsql(
            "Host=localhost;Database=sabormercado;Username=sabormercado;Password=changeme",
            npgsql => npgsql.MigrationsHistoryTable("__ef_migrations", "identity"));

        return new IdentityDbContext(options.Options);
    }
}
