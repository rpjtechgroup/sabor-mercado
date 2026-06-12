using Microsoft.EntityFrameworkCore;

namespace SaborMercado.Modules.Rewards.Data;

public sealed class RewardsDbContext(DbContextOptions<RewardsDbContext> options) : DbContext(options)
{
    public DbSet<CreditLedgerEntry> CreditLedgerEntries => Set<CreditLedgerEntry>();

    public DbSet<FeatureUnlock> FeatureUnlocks => Set<FeatureUnlock>();

    public DbSet<UserAchievement> UserAchievements => Set<UserAchievement>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("rewards");

        modelBuilder.Entity<CreditLedgerEntry>(entity =>
        {
            entity.ToTable("rewards_credit_ledger");
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.UserId);
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            entity.Property(e => e.ReferenceId).HasColumnName("reference_id");
        });

        modelBuilder.Entity<FeatureUnlock>(entity =>
        {
            entity.ToTable("rewards_feature_unlocks");
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.UserId, e.FeatureCode });
            entity.Property(e => e.UnlockedAt).HasColumnName("unlocked_at");
            entity.Property(e => e.ExpiresAt).HasColumnName("expires_at");
        });

        modelBuilder.Entity<UserAchievement>(entity =>
        {
            entity.ToTable("rewards_user_achievements");
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.UserId, e.AchievementCode }).IsUnique();
            entity.Property(e => e.AchievementCode).HasColumnName("achievement_code");
            entity.Property(e => e.UnlockedAt).HasColumnName("unlocked_at");
        });
    }
}
