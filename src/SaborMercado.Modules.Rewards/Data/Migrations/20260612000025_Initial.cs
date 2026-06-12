using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SaborMercado.Modules.Rewards.Data.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "rewards");

            migrationBuilder.CreateTable(
                name: "rewards_credit_ledger",
                schema: "rewards",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Amount = table.Column<int>(type: "integer", nullable: false),
                    Reason = table.Column<int>(type: "integer", nullable: false),
                    reference_id = table.Column<Guid>(type: "uuid", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
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
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    FeatureCode = table.Column<string>(type: "text", nullable: false),
                    unlocked_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    expires_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "rewards_credit_ledger",
                schema: "rewards");

            migrationBuilder.DropTable(
                name: "rewards_feature_unlocks",
                schema: "rewards");
        }
    }
}
