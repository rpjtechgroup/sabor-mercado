using Microsoft.EntityFrameworkCore;

namespace SaborMercado.Modules.SharedCatalog.Data;

public sealed class SharedCatalogDbContext(DbContextOptions<SharedCatalogDbContext> options) : DbContext(options)
{
    public DbSet<Market> Markets => Set<Market>();

    public DbSet<SharedProduct> SharedProducts => Set<SharedProduct>();

    public DbSet<PriceObservation> PriceObservations => Set<PriceObservation>();

    public DbSet<ContributionIdempotency> ContributionIdempotencies => Set<ContributionIdempotency>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("shared_catalog");
        modelBuilder.Entity<Market>(entity =>
        {
            entity.ToTable("shared_markets");
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.Name, e.City, e.State });
        });

        modelBuilder.Entity<SharedProduct>(entity =>
        {
            entity.ToTable("shared_products");
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Ean);
            entity.HasIndex(e => e.NormalizedName);
        });

        modelBuilder.Entity<PriceObservation>(entity =>
        {
            entity.ToTable("shared_price_observations");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Price).HasColumnType("TEXT");
            entity.Property(e => e.ContributorPseudonymId).HasColumnName("contributor_pseudonym_id");
            entity.HasIndex(e => new { e.ContributorPseudonymId, e.ObservedOn });
            entity.HasIndex(e => new { e.SharedProductId, e.MarketId, e.ObservedOn });
        });

        modelBuilder.Entity<ContributionIdempotency>(entity =>
        {
            entity.ToTable("contribution_idempotency");
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.AccountId, e.IdempotencyKey }).IsUnique();
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
        });
    }
}
