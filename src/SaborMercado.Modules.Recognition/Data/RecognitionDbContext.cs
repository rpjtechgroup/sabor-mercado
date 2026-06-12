using Microsoft.EntityFrameworkCore;

namespace SaborMercado.Modules.Recognition.Data;

public sealed class RecognitionDbContext(DbContextOptions<RecognitionDbContext> options) : DbContext(options)
{
    public DbSet<RecognitionLog> RecognitionLogs => Set<RecognitionLog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("recognition");

        modelBuilder.Entity<RecognitionLog>(entity =>
        {
            entity.ToTable("recognition_logs");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            entity.Property(e => e.Succeeded).HasColumnName("succeeded");
            entity.Property(e => e.FailureReason).HasColumnName("failure_reason");
            entity.Property(e => e.LatencyMs).HasColumnName("latency_ms");
            entity.Property(e => e.ClientKey).HasColumnName("client_key");
        });
    }
}
