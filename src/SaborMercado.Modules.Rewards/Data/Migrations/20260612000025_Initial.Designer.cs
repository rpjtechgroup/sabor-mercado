
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using SaborMercado.Modules.Rewards.Data;

#nullable disable

namespace SaborMercado.Modules.Rewards.Data.Migrations
{
    [DbContext(typeof(RewardsDbContext))]
    [Migration("20260612000025_Initial")]
    partial class Initial
    {
        
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasDefaultSchema("rewards")
                .HasAnnotation("ProductVersion", "8.0.22")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("SaborMercado.Modules.Rewards.Data.CreditLedgerEntry", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<int>("Amount")
                        .HasColumnType("integer");

                    b.Property<DateTimeOffset>("CreatedAt")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("created_at");

                    b.Property<int>("Reason")
                        .HasColumnType("integer");

                    b.Property<Guid?>("ReferenceId")
                        .HasColumnType("uuid")
                        .HasColumnName("reference_id");

                    b.Property<Guid>("UserId")
                        .HasColumnType("uuid");

                    b.HasKey("Id");

                    b.HasIndex("UserId");

                    b.ToTable("rewards_credit_ledger", "rewards");
                });

            modelBuilder.Entity("SaborMercado.Modules.Rewards.Data.FeatureUnlock", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<DateTimeOffset?>("ExpiresAt")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("expires_at");

                    b.Property<string>("FeatureCode")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<DateTimeOffset>("UnlockedAt")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("unlocked_at");

                    b.Property<Guid>("UserId")
                        .HasColumnType("uuid");

                    b.HasKey("Id");

                    b.HasIndex("UserId", "FeatureCode");

                    b.ToTable("rewards_feature_unlocks", "rewards");
                });
#pragma warning restore 612, 618
        }
    }
}
