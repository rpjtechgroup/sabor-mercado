using Microsoft.EntityFrameworkCore;

namespace SaborMercado.Modules.Rewards.Data;

public sealed class RewardsDbContext(DbContextOptions<RewardsDbContext> options) : DbContext(options)
{
    public DbSet<UserAchievement> UserAchievements => Set<UserAchievement>();

    public DbSet<UserGamificationMetrics> UserGamificationMetrics => Set<UserGamificationMetrics>();

    public DbSet<RankingSnapshot> RankingSnapshots => Set<RankingSnapshot>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("rewards");

        modelBuilder.Entity<UserAchievement>(entity =>
        {
            entity.ToTable("rewards_user_achievements");
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.UserId, e.AchievementCode }).IsUnique();
            entity.Property(e => e.AchievementCode).HasColumnName("achievement_code");
            entity.Property(e => e.UnlockedAt).HasColumnName("unlocked_at");
        });

        modelBuilder.Entity<UserGamificationMetrics>(entity =>
        {
            entity.ToTable("rewards_user_metrics");
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.UserId).IsUnique();
            entity.Property(e => e.LastLoginAt).HasColumnName("last_login_at");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");
        });

        modelBuilder.Entity<RankingSnapshot>(entity =>
        {
            entity.ToTable("rewards_ranking_snapshots");
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.RankingType, e.RankPosition });
            entity.HasIndex(e => new { e.RankingType, e.UserId });
            entity.Property(e => e.RankingType).HasColumnName("ranking_type");
            entity.Property(e => e.PseudonymDisplay).HasColumnName("pseudonym_display");
            entity.Property(e => e.RankPosition).HasColumnName("rank_position");
            entity.Property(e => e.CalculatedAt).HasColumnName("calculated_at");
        });
    }
}
