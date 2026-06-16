using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SaborMercado.Modules.Identity.Data.Migrations;

public partial class GoogleSubjectId : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string>(
            name: "google_subject_id",
            schema: "identity",
            table: "identity_users",
            type: "character varying(64)",
            maxLength: 64,
            nullable: true);

        migrationBuilder.CreateIndex(
            name: "IX_identity_users_google_subject_id",
            schema: "identity",
            table: "identity_users",
            column: "google_subject_id",
            unique: true);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropIndex(
            name: "IX_identity_users_google_subject_id",
            schema: "identity",
            table: "identity_users");

        migrationBuilder.DropColumn(
            name: "google_subject_id",
            schema: "identity",
            table: "identity_users");
    }
}
