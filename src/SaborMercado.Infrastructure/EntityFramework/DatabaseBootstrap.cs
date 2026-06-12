using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace SaborMercado.Infrastructure.EntityFramework;

public static class DatabaseBootstrap
{
    public const string ProviderKey = "Database:Provider";
    public const string SqliteProvider = "Sqlite";
    public const string PostgreSqlProvider = "PostgreSQL";

    public static void ConfigureDbContext<TContext>(
        DbContextOptionsBuilder options,
        IConfiguration configuration,
        string connectionStringName,
        string? postgresSchema = null)
        where TContext : DbContext
    {
        var connectionString = configuration.GetConnectionString(connectionStringName)
            ?? throw new InvalidOperationException($"Connection string '{connectionStringName}' is required.");

        if (IsPostgreSql(configuration))
        {
            options.UseNpgsql(connectionString, npgsql =>
            {
                if (postgresSchema is not null)
                {
                    npgsql.MigrationsHistoryTable("__ef_migrations", postgresSchema);
                }
            });
        }
        else
        {
            options.UseSqlite(connectionString);
        }
    }

    public static async Task InitializeModuleAsync<TContext>(WebApplication app)
        where TContext : DbContext
    {
        await using var scope = app.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<TContext>();
        var configuration = scope.ServiceProvider.GetRequiredService<IConfiguration>();

        if (IsPostgreSql(configuration))
        {
            await db.Database.MigrateAsync();
        }
        else
        {
            await db.Database.EnsureCreatedAsync();
        }
    }

    public static bool IsPostgreSql(IConfiguration configuration) =>
        string.Equals(
            configuration[ProviderKey],
            PostgreSqlProvider,
            StringComparison.OrdinalIgnoreCase);
}
