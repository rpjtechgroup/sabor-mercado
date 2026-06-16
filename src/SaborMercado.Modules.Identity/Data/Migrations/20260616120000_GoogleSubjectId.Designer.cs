using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using SaborMercado.Modules.Identity.Data;

#nullable disable

namespace SaborMercado.Modules.Identity.Data.Migrations;

[DbContext(typeof(IdentityDbContext))]
[Migration("20260616120000_GoogleSubjectId")]
partial class GoogleSubjectId
{
    protected override void BuildTargetModel(ModelBuilder modelBuilder)
    {
#pragma warning disable 612, 618
        modelBuilder
            .HasDefaultSchema("identity")
            .HasAnnotation("ProductVersion", "8.0.22")
            .HasAnnotation("Relational:MaxIdentifierLength", 63);

        NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

        modelBuilder.Entity("SaborMercado.Modules.Identity.Data.RefreshToken", b =>
            {
                b.Property<Guid>("Id")
                    .ValueGeneratedOnAdd()
                    .HasColumnType("uuid");

                b.Property<DateTimeOffset>("CreatedAt")
                    .HasColumnType("timestamp with time zone")
                    .HasColumnName("created_at");

                b.Property<DateTimeOffset>("ExpiresAt")
                    .HasColumnType("timestamp with time zone")
                    .HasColumnName("expires_at");

                b.Property<string>("TokenHash")
                    .IsRequired()
                    .HasColumnType("text");

                b.Property<Guid>("UserId")
                    .HasColumnType("uuid");

                b.HasKey("Id");

                b.HasIndex("TokenHash")
                    .IsUnique();

                b.HasIndex("UserId");

                b.ToTable("identity_refresh_tokens", "identity");
            });

        modelBuilder.Entity("SaborMercado.Modules.Identity.Data.UserAccount", b =>
            {
                b.Property<Guid>("Id")
                    .ValueGeneratedOnAdd()
                    .HasColumnType("uuid");

                b.Property<DateTimeOffset>("CreatedAt")
                    .HasColumnType("timestamp with time zone")
                    .HasColumnName("created_at");

                b.Property<string>("Email")
                    .IsRequired()
                    .HasMaxLength(256)
                    .HasColumnType("character varying(256)");

                b.Property<string>("GoogleSubjectId")
                    .HasMaxLength(64)
                    .HasColumnType("character varying(64)")
                    .HasColumnName("google_subject_id");

                b.Property<string>("PasswordHash")
                    .IsRequired()
                    .HasMaxLength(512)
                    .HasColumnType("character varying(512)");

                b.Property<Guid>("PseudonymId")
                    .HasColumnType("uuid")
                    .HasColumnName("pseudonym_id");

                b.HasKey("Id");

                b.HasIndex("Email")
                    .IsUnique();

                b.HasIndex("GoogleSubjectId")
                    .IsUnique();

                b.ToTable("identity_users", "identity");
            });
#pragma warning restore 612, 618
    }
}
