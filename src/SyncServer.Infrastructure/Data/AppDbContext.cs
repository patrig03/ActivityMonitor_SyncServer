using Microsoft.EntityFrameworkCore;
using SyncServer.Core.Domain.Entities;

namespace SyncServer.Infrastructure.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<Device> Devices => Set<Device>();
    public DbSet<Application> Applications => Set<Application>();
    public DbSet<Session> Sessions => Set<Session>();
    public DbSet<Activity> Activities => Set<Activity>();
    public DbSet<Threshold> Thresholds => Set<Threshold>();
    public DbSet<UserSetting> UserSettings => Set<UserSetting>();
    public DbSet<Category> Categories => Set<Category>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Email).IsUnique();
            entity.Property(e => e.Email).HasMaxLength(255).IsRequired();
            entity.Property(e => e.PasswordHash).IsRequired();
        });

        modelBuilder.Entity<Device>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.UserId);
            entity.Property(e => e.Name).HasMaxLength(255).IsRequired();
        });

        modelBuilder.Entity<Application>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.UserId);
            entity.Property(e => e.Name).HasMaxLength(255).IsRequired();
        });

        modelBuilder.Entity<Session>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.UserId);
            entity.HasIndex(e => e.DeviceId);
            entity.HasIndex(e => e.ApplicationId);
        });

        modelBuilder.Entity<Activity>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.UserId);
            entity.HasIndex(e => e.DeviceId);
            entity.HasIndex(e => e.ApplicationId);
            entity.Property(e => e.Url).HasMaxLength(2048);
        });

        modelBuilder.Entity<Threshold>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.UserId);
            entity.Property(e => e.InterventionType).HasMaxLength(50).IsRequired();
        });

        modelBuilder.Entity<UserSetting>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.UserId);
            entity.HasIndex(e => new { e.UserId, e.Key }).IsUnique();
            entity.Property(e => e.Key).HasMaxLength(255).IsRequired();
            entity.Property(e => e.Value).HasMaxLength(4096).IsRequired();
        });

        modelBuilder.Entity<Category>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.UserId);
            entity.Property(e => e.Name).HasMaxLength(255).IsRequired();
        });
    }
}