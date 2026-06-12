
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using SaborMercado.Modules.SharedCatalog.Data;

#nullable disable

namespace SaborMercado.Modules.SharedCatalog.Data.Migrations
{
    [DbContext(typeof(SharedCatalogDbContext))]
    [Migration("20260612185111_CommunityTrust")]
    partial class CommunityTrust
    {
        
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasDefaultSchema("shared_catalog")
                .HasAnnotation("ProductVersion", "8.0.22")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("SaborMercado.Modules.SharedCatalog.Data.ContributionIdempotency", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<Guid>("AccountId")
                        .HasColumnType("uuid");

                    b.Property<DateTimeOffset>("CreatedAt")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("created_at");

                    b.Property<string>("IdempotencyKey")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("RequestHash")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("ResponseJson")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.HasIndex("AccountId", "IdempotencyKey")
                        .IsUnique();

                    b.ToTable("contribution_idempotency", "shared_catalog");
                });

            modelBuilder.Entity("SaborMercado.Modules.SharedCatalog.Data.ContributorReport", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<DateTimeOffset>("CreatedAt")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("created_at");

                    b.Property<string>("Details")
                        .HasColumnType("text");

                    b.Property<Guid?>("ObservationId")
                        .HasColumnType("uuid");

                    b.Property<string>("Reason")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<Guid>("ReporterUserId")
                        .HasColumnType("uuid");

                    b.Property<Guid>("TargetPseudonymId")
                        .HasColumnType("uuid");

                    b.HasKey("Id");

                    b.HasIndex("TargetPseudonymId", "CreatedAt");

                    b.HasIndex("ReporterUserId", "TargetPseudonymId", "ObservationId", "Reason");

                    b.ToTable("contributor_reports", "shared_catalog");
                });

            modelBuilder.Entity("SaborMercado.Modules.SharedCatalog.Data.ContributorTrust", b =>
                {
                    b.Property<Guid>("PseudonymId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<int>("AcceptedContributions")
                        .HasColumnType("integer");

                    b.Property<Guid?>("ContributorUserId")
                        .HasColumnType("uuid");

                    b.Property<bool>("IsRestricted")
                        .HasColumnType("boolean");

                    b.Property<int>("ReportCount")
                        .HasColumnType("integer");

                    b.Property<DateTimeOffset?>("RestrictedUntil")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("restricted_until");

                    b.Property<int>("TotalDownvotesReceived")
                        .HasColumnType("integer");

                    b.Property<int>("TotalUpvotesReceived")
                        .HasColumnType("integer");

                    b.Property<int>("TrustScore")
                        .HasColumnType("integer");

                    b.Property<DateTimeOffset>("UpdatedAt")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("updated_at");

                    b.HasKey("PseudonymId");

                    b.ToTable("contributor_trust", "shared_catalog");
                });

            modelBuilder.Entity("SaborMercado.Modules.SharedCatalog.Data.Market", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<string>("City")
                        .HasColumnType("text");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("State")
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.HasIndex("Name", "City", "State");

                    b.ToTable("shared_markets", "shared_catalog");
                });

            modelBuilder.Entity("SaborMercado.Modules.SharedCatalog.Data.ObservationVote", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<DateTimeOffset>("CreatedAt")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("created_at");

                    b.Property<Guid>("ObservationId")
                        .HasColumnType("uuid");

                    b.Property<DateTimeOffset>("UpdatedAt")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("updated_at");

                    b.Property<int>("Value")
                        .HasColumnType("integer");

                    b.Property<Guid>("VoterUserId")
                        .HasColumnType("uuid");

                    b.HasKey("Id");

                    b.HasIndex("ObservationId", "VoterUserId")
                        .IsUnique();

                    b.ToTable("observation_votes", "shared_catalog");
                });

            modelBuilder.Entity("SaborMercado.Modules.SharedCatalog.Data.PriceObservation", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<Guid>("ContributorPseudonymId")
                        .HasColumnType("uuid")
                        .HasColumnName("contributor_pseudonym_id");

                    b.Property<DateTimeOffset>("CreatedAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<int>("DownvoteCount")
                        .HasColumnType("integer");

                    b.Property<bool>("IsHidden")
                        .HasColumnType("boolean");

                    b.Property<Guid>("MarketId")
                        .HasColumnType("uuid");

                    b.Property<DateOnly>("ObservedOn")
                        .HasColumnType("date");

                    b.Property<string>("Price")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("RejectionReason")
                        .HasColumnType("text");

                    b.Property<Guid>("SharedProductId")
                        .HasColumnType("uuid");

                    b.Property<int>("Status")
                        .HasColumnType("integer");

                    b.Property<int>("UpvoteCount")
                        .HasColumnType("integer");

                    b.HasKey("Id");

                    b.HasIndex("MarketId");

                    b.HasIndex("ContributorPseudonymId", "ObservedOn");

                    b.HasIndex("SharedProductId", "MarketId", "ObservedOn");

                    b.ToTable("shared_price_observations", "shared_catalog");
                });

            modelBuilder.Entity("SaborMercado.Modules.SharedCatalog.Data.SharedProduct", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<string>("Brand")
                        .HasColumnType("text");

                    b.Property<string>("Category")
                        .HasColumnType("text");

                    b.Property<string>("Ean")
                        .HasColumnType("text");

                    b.Property<string>("NormalizedName")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("QuantityUnit")
                        .HasColumnType("text");

                    b.Property<decimal?>("QuantityValue")
                        .HasColumnType("numeric");

                    b.HasKey("Id");

                    b.HasIndex("Ean");

                    b.HasIndex("NormalizedName");

                    b.ToTable("shared_products", "shared_catalog");
                });

            modelBuilder.Entity("SaborMercado.Modules.SharedCatalog.Data.ObservationVote", b =>
                {
                    b.HasOne("SaborMercado.Modules.SharedCatalog.Data.PriceObservation", "Observation")
                        .WithMany()
                        .HasForeignKey("ObservationId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Observation");
                });

            modelBuilder.Entity("SaborMercado.Modules.SharedCatalog.Data.PriceObservation", b =>
                {
                    b.HasOne("SaborMercado.Modules.SharedCatalog.Data.Market", "Market")
                        .WithMany()
                        .HasForeignKey("MarketId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("SaborMercado.Modules.SharedCatalog.Data.SharedProduct", "SharedProduct")
                        .WithMany()
                        .HasForeignKey("SharedProductId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Market");

                    b.Navigation("SharedProduct");
                });
#pragma warning restore 612, 618
        }
    }
}
