using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace SaborMercado.Modules.SharedCatalog.Data;

/// <summary>
/// Design-time factory for EF migrations targeting PostgreSQL (production provider).
/// </summary>
public sealed class SharedCatalogDbContextFactory : IDesignTimeDbContextFactory<SharedCatalogDbContext>
{
    public SharedCatalogDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<SharedCatalogDbContext>();
        options.UseNpgsql(
            "Host=localhost;Database=sabormercado;Username=sabormercado;Password=changeme",
            npgsql => npgsql.MigrationsHistoryTable("__ef_migrations", "shared_catalog"));

        return new SharedCatalogDbContext(options.Options);
    }
}
