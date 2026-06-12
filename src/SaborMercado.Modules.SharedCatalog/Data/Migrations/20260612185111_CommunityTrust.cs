using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SaborMercado.Modules.SharedCatalog.Data.Migrations
{
    
    public partial class CommunityTrust : Migration
    {
        
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "DownvoteCount",
                schema: "shared_catalog",
                table: "shared_price_observations",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "IsHidden",
                schema: "shared_catalog",
                table: "shared_price_observations",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "UpvoteCount",
                schema: "shared_catalog",
                table: "shared_price_observations",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "contributor_reports",
                schema: "shared_catalog",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ReporterUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    TargetPseudonymId = table.Column<Guid>(type: "uuid", nullable: false),
                    ObservationId = table.Column<Guid>(type: "uuid", nullable: true),
                    Reason = table.Column<string>(type: "text", nullable: false),
                    Details = table.Column<string>(type: "text", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_contributor_reports", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "contributor_trust",
                schema: "shared_catalog",
                columns: table => new
                {
                    PseudonymId = table.Column<Guid>(type: "uuid", nullable: false),
                    ContributorUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    TrustScore = table.Column<int>(type: "integer", nullable: false),
                    TotalUpvotesReceived = table.Column<int>(type: "integer", nullable: false),
                    TotalDownvotesReceived = table.Column<int>(type: "integer", nullable: false),
                    AcceptedContributions = table.Column<int>(type: "integer", nullable: false),
                    ReportCount = table.Column<int>(type: "integer", nullable: false),
                    IsRestricted = table.Column<bool>(type: "boolean", nullable: false),
                    restricted_until = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_contributor_trust", x => x.PseudonymId);
                });

            migrationBuilder.CreateTable(
                name: "observation_votes",
                schema: "shared_catalog",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ObservationId = table.Column<Guid>(type: "uuid", nullable: false),
                    VoterUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Value = table.Column<int>(type: "integer", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_observation_votes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_observation_votes_shared_price_observations_ObservationId",
                        column: x => x.ObservationId,
                        principalSchema: "shared_catalog",
                        principalTable: "shared_price_observations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_contributor_reports_ReporterUserId_TargetPseudonymId_Observ~",
                schema: "shared_catalog",
                table: "contributor_reports",
                columns: new[] { "ReporterUserId", "TargetPseudonymId", "ObservationId", "Reason" });

            migrationBuilder.CreateIndex(
                name: "IX_contributor_reports_TargetPseudonymId_created_at",
                schema: "shared_catalog",
                table: "contributor_reports",
                columns: new[] { "TargetPseudonymId", "created_at" });

            migrationBuilder.CreateIndex(
                name: "IX_observation_votes_ObservationId_VoterUserId",
                schema: "shared_catalog",
                table: "observation_votes",
                columns: new[] { "ObservationId", "VoterUserId" },
                unique: true);
        }

        
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "contributor_reports",
                schema: "shared_catalog");

            migrationBuilder.DropTable(
                name: "contributor_trust",
                schema: "shared_catalog");

            migrationBuilder.DropTable(
                name: "observation_votes",
                schema: "shared_catalog");

            migrationBuilder.DropColumn(
                name: "DownvoteCount",
                schema: "shared_catalog",
                table: "shared_price_observations");

            migrationBuilder.DropColumn(
                name: "IsHidden",
                schema: "shared_catalog",
                table: "shared_price_observations");

            migrationBuilder.DropColumn(
                name: "UpvoteCount",
                schema: "shared_catalog",
                table: "shared_price_observations");
        }
    }
}
