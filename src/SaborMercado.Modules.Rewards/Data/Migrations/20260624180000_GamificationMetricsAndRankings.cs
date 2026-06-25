using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SaborMercado.Modules.Rewards.Data.Migrations
{
    public partial class GamificationMetricsAndRankings : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "rewards_credit_ledger",
                schema: "rewards");

            migrationBuilder.DropTable(
                name: "rewards_feature_unlocks",
                schema: "rewards");

            migrationBuilder.CreateTable(
                name: "rewards_user_metrics",
                schema: "rewards",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    TotalProductsRegistered = table.Column<int>(type: "integer", nullable: false),
                    TotalStoresRegistered = table.Column<int>(type: "integer", nullable: false),
                    TotalPurchasesCompleted = table.Column<int>(type: "integer", nullable: false),
                    TotalPurchasesWithBudgetOk = table.Column<int>(type: "integer", nullable: false),
                    TotalOcrItemsAdded = table.Column<int>(type: "integer", nullable: false),
                    TotalProductsWithPriceHistory = table.Column<int>(type: "integer", nullable: false),
                    CurrentLoginStreakDays = table.Column<int>(type: "integer", nullable: false),
                    LongestLoginStreakDays = table.Column<int>(type: "integer", nullable: false),
                    last_login_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_rewards_user_metrics", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "rewards_ranking_snapshots",
                schema: "rewards",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ranking_type = table.Column<string>(type: "text", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    pseudonym_display = table.Column<string>(type: "text", nullable: false),
                    rank_position = table.Column<int>(type: "integer", nullable: false),
                    Score = table.Column<int>(type: "integer", nullable: false),
                    calculated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_rewards_ranking_snapshots", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_rewards_user_metrics_UserId",
                schema: "rewards",
                table: "rewards_user_metrics",
                column: "UserId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_rewards_ranking_snapshots_ranking_type_rank_position",
                schema: "rewards",
                table: "rewards_ranking_snapshots",
                columns: new[] { "ranking_type", "rank_position" });

            migrationBuilder.CreateIndex(
                name: "IX_rewards_ranking_snapshots_ranking_type_UserId",
                schema: "rewards",
                table: "rewards_ranking_snapshots",
                columns: new[] { "ranking_type", "UserId" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "rewards_ranking_snapshots",
                schema: "rewards");

            migrationBuilder.DropTable(
                name: "rewards_user_metrics",
                schema: "rewards");

            migrationBuilder.CreateTable(
                name: "rewards_credit_ledger",
                schema: "rewards",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Amount = table.Column<int>(type: "integer", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    Reason = table.Column<int>(type: "integer", nullable: false),
                    reference_id = table.Column<Guid>(type: "uuid", nullable: true),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_rewards_credit_ledger", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "rewards_feature_unlocks",
                schema: "rewards",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    expires_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    FeatureCode = table.Column<string>(type: "text", nullable: false),
                    unlocked_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_rewards_feature_unlocks", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_rewards_credit_ledger_UserId",
                schema: "rewards",
                table: "rewards_credit_ledger",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_rewards_feature_unlocks_UserId_FeatureCode",
                schema: "rewards",
                table: "rewards_feature_unlocks",
                columns: new[] { "UserId", "FeatureCode" });
        }
    }
}
