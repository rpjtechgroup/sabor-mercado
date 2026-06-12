using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SaborMercado.Modules.Rewards.Data.Migrations
{
    
    public partial class UserAchievements : Migration
    {
        
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "rewards_user_achievements",
                schema: "rewards",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    achievement_code = table.Column<string>(type: "text", nullable: false),
                    unlocked_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_rewards_user_achievements", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_rewards_user_achievements_UserId_achievement_code",
                schema: "rewards",
                table: "rewards_user_achievements",
                columns: new[] { "UserId", "achievement_code" },
                unique: true);
        }

        
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "rewards_user_achievements",
                schema: "rewards");
        }
    }
}
