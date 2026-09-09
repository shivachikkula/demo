using DistrictPortal.Api.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace DistrictPortal.Api.Data;

public sealed class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Lea> Leas => Set<Lea>();

    public DbSet<CollectionDefinition> Collections => Set<CollectionDefinition>();

    public DbSet<Submission> Submissions => Set<Submission>();

    public DbSet<NotificationEntity> Notifications => Set<NotificationEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Lea>(lea =>
        {
            lea.HasKey(l => l.Id);
            lea.HasMany(l => l.Collections).WithOne(c => c.Lea).HasForeignKey(c => c.LeaId);
        });

        modelBuilder.Entity<CollectionDefinition>(collection =>
        {
            collection.HasKey(c => c.Id);
            collection.HasMany(c => c.Submissions).WithOne(s => s.Collection).HasForeignKey(s => s.CollectionId);
        });

        modelBuilder.Entity<Submission>(submission =>
        {
            submission.HasKey(s => s.Id);
            submission.Property(s => s.Status).HasConversion<string>();
        });

        modelBuilder.Entity<NotificationEntity>(notification =>
        {
            notification.HasKey(n => n.Id);
            notification.Property(n => n.Severity).HasConversion<string>();
        });
    }
}
