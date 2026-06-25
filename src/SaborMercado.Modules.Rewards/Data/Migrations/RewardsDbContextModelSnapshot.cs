
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using SaborMercado.Modules.Rewards.Data;

#nullable disable

namespace SaborMercado.Modules.Rewards.Data.Migrations
{
    [DbContext(typeof(RewardsDbContext))]
    partial class RewardsDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasDefaultSchema("rewards")
                .HasAnnotation("ProductVersion", "8.0.22")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("SaborMercado.Modules.Rewards.Data.RankingSnapshot", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<DateTimeOffset>("CalculatedAt")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("calculated_at");

                    b.Property<string>("PseudonymDisplay")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("pseudonym_display");

                    b.Property<int>("RankPosition")
                        .HasColumnType("integer")
                        .HasColumnName("rank_position");

                    b.Property<string>("RankingType")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("ranking_type");

                    b.Property<int>("Score")
                        .HasColumnType("integer");

                    b.Property<Guid>("UserId")
                        .HasColumnType("uuid");

                    b.HasKey("Id");

                    b.HasIndex("RankingType", "RankPosition");

                    b.HasIndex("RankingType", "UserId");

                    b.ToTable("rewards_ranking_snapshots", "rewards");
                });

            modelBuilder.Entity("SaborMercado.Modules.Rewards.Data.UserAchievement", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<string>("AchievementCode")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("achievement_code");

                    b.Property<DateTimeOffset>("UnlockedAt")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("unlocked_at");

                    b.Property<Guid>("UserId")
                        .HasColumnType("uuid");

                    b.HasKey("Id");

                    b.HasIndex("UserId", "AchievementCode")
                        .IsUnique();

                    b.ToTable("rewards_user_achievements", "rewards");
                });

            modelBuilder.Entity("SaborMercado.Modules.Rewards.Data.UserGamificationMetrics", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<int>("CurrentLoginStreakDays")
                        .HasColumnType("integer");

                    b.Property<DateTimeOffset?>("LastLoginAt")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("last_login_at");

                    b.Property<int>("LongestLoginStreakDays")
                        .HasColumnType("integer");

                    b.Property<int>("TotalOcrItemsAdded")
                        .HasColumnType("integer");

                    b.Property<int>("TotalProductsRegistered")
                        .HasColumnType("integer");

                    b.Property<int>("TotalProductsWithPriceHistory")
                        .HasColumnType("integer");

                    b.Property<int>("TotalPurchasesCompleted")
                        .HasColumnType("integer");

                    b.Property<int>("TotalPurchasesWithBudgetOk")
                        .HasColumnType("integer");

                    b.Property<int>("TotalStoresRegistered")
                        .HasColumnType("integer");

                    b.Property<DateTimeOffset>("UpdatedAt")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("updated_at");

                    b.Property<Guid>("UserId")
                        .HasColumnType("uuid");

                    b.HasKey("Id");

                    b.HasIndex("UserId")
                        .IsUnique();

                    b.ToTable("rewards_user_metrics", "rewards");
                });
#pragma warning restore 612, 618
        }
    }
}
