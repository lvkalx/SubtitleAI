using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SubtitleAI.Domain;

namespace SubtitleAI.Infrastructure.Persistence;

public class AppDbContext(DbContextOptions<AppDbContext> options)
    : IdentityDbContext<ApplicationUser>(options)
{
    public DbSet<MediaJob> MediaJobs => Set<MediaJob>();
    public DbSet<Transcript> Transcripts => Set<Transcript>();
    public DbSet<Summary> Summaries => Set<Summary>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        base.OnModelCreating(b);

        b.Entity<MediaJob>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.OriginalFileName).HasMaxLength(500).IsRequired();
            e.Property(x => x.BlobPath).HasMaxLength(1000).IsRequired();
            e.Property(x => x.Status).HasConversion<string>().HasMaxLength(20);
            e.HasIndex(x => new { x.UserId, x.CreatedAt });

            e.HasOne<ApplicationUser>().WithMany()
                .HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Cascade);

            e.HasOne(x => x.Transcript).WithOne()
                .HasForeignKey<Transcript>(t => t.JobId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(x => x.Summary).WithOne()
                .HasForeignKey<Summary>(s => s.JobId).OnDelete(DeleteBehavior.Cascade);
        });

        b.Entity<Transcript>().HasKey(x => x.JobId);
        b.Entity<Summary>().HasKey(x => x.JobId);
        b.Entity<RefreshToken>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.TokenHash).HasMaxLength(64).IsRequired();
            e.HasIndex(x => x.TokenHash).IsUnique();
            e.HasOne<ApplicationUser>().WithMany()
                .HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Cascade);
        });
    }
}