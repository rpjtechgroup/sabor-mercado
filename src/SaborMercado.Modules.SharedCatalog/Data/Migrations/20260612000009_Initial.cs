using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SaborMercado.Modules.SharedCatalog.Data.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "shared_catalog");

            migrationBuilder.CreateTable(
                name: "contribution_idempotency",
                schema: "shared_catalog",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AccountId = table.Column<Guid>(type: "uuid", nullable: false),
                    IdempotencyKey = table.Column<string>(type: "text", nullable: false),
                    RequestHash = table.Column<string>(type: "text", nullable: false),
                    ResponseJson = table.Column<string>(type: "text", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_contribution_idempotency", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "shared_markets",
                schema: "shared_catalog",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    City = table.Column<string>(type: "text", nullable: true),
                    State = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_shared_markets", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "shared_products",
                schema: "shared_catalog",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    NormalizedName = table.Column<string>(type: "text", nullable: false),
                    Brand = table.Column<string>(type: "text", nullable: true),
                    Ean = table.Column<string>(type: "text", nullable: true),
                    QuantityValue = table.Column<decimal>(type: "numeric", nullable: true),
                    QuantityUnit = table.Column<string>(type: "text", nullable: true),
                    Category = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_shared_products", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "shared_price_observations",
                schema: "shared_catalog",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SharedProductId = table.Column<Guid>(type: "uuid", nullable: false),
                    MarketId = table.Column<Guid>(type: "uuid", nullable: false),
                    Price = table.Column<string>(type: "TEXT", nullable: false),
                    ObservedOn = table.Column<DateOnly>(type: "date", nullable: false),
                    contributor_pseudonym_id = table.Column<Guid>(type: "uuid", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    RejectionReason = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_shared_price_observations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_shared_price_observations_shared_markets_MarketId",
                        column: x => x.MarketId,
                        principalSchema: "shared_catalog",
                        principalTable: "shared_markets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_shared_price_observations_shared_products_SharedProductId",
                        column: x => x.SharedProductId,
                        principalSchema: "shared_catalog",
                        principalTable: "shared_products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_contribution_idempotency_AccountId_IdempotencyKey",
                schema: "shared_catalog",
                table: "contribution_idempotency",
                columns: new[] { "AccountId", "IdempotencyKey" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_shared_markets_Name_City_State",
                schema: "shared_catalog",
                table: "shared_markets",
                columns: new[] { "Name", "City", "State" });

            migrationBuilder.CreateIndex(
                name: "IX_shared_price_observations_contributor_pseudonym_id_Observed~",
                schema: "shared_catalog",
                table: "shared_price_observations",
                columns: new[] { "contributor_pseudonym_id", "ObservedOn" });

            migrationBuilder.CreateIndex(
                name: "IX_shared_price_observations_MarketId",
                schema: "shared_catalog",
                table: "shared_price_observations",
                column: "MarketId");

            migrationBuilder.CreateIndex(
                name: "IX_shared_price_observations_SharedProductId_MarketId_Observed~",
                schema: "shared_catalog",
                table: "shared_price_observations",
                columns: new[] { "SharedProductId", "MarketId", "ObservedOn" });

            migrationBuilder.CreateIndex(
                name: "IX_shared_products_Ean",
                schema: "shared_catalog",
                table: "shared_products",
                column: "Ean");

            migrationBuilder.CreateIndex(
                name: "IX_shared_products_NormalizedName",
                schema: "shared_catalog",
                table: "shared_products",
                column: "NormalizedName");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "contribution_idempotency",
                schema: "shared_catalog");

            migrationBuilder.DropTable(
                name: "shared_price_observations",
                schema: "shared_catalog");

            migrationBuilder.DropTable(
                name: "shared_markets",
                schema: "shared_catalog");

            migrationBuilder.DropTable(
                name: "shared_products",
                schema: "shared_catalog");
        }
    }
}
