using Microsoft.EntityFrameworkCore;

namespace SaborMercado.Modules.Identity.Data;

public sealed class IdentityDbContext(DbContextOptions<IdentityDbContext> options) : DbContext(options)
{
    public DbSet<UserAccount> Users => Set<UserAccount>();

    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("identity");

        modelBuilder.Entity<UserAccount>(entity =>
        {
            entity.ToTable("identity_users");
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Email).IsUnique();
            entity.Property(e => e.Email).HasMaxLength(256);
            entity.Property(e => e.PasswordHash).HasMaxLength(512);
            entity.HasIndex(e => e.GoogleSubjectId).IsUnique();
            entity.Property(e => e.GoogleSubjectId).HasMaxLength(64).HasColumnName("google_subject_id");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            entity.Property(e => e.PseudonymId).HasColumnName("pseudonym_id");
        });

        modelBuilder.Entity<RefreshToken>(entity =>
        {
            entity.ToTable("identity_refresh_tokens");
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.UserId);
            entity.HasIndex(e => e.TokenHash).IsUnique();
            entity.Property(e => e.ExpiresAt).HasColumnName("expires_at");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
        });
    }
}
