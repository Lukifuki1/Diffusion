using Microsoft.EntityFrameworkCore;
using AuraFlow.Core.Models.Settings;

namespace AuraFlow.Infrastructure.Persistence;

/// <summary>
/// Main database context for AuraFlow Studio
/// </summary>
public class AuraFlowDbContext : DbContext
{
    public AuraFlowDbContext(DbContextOptions<AuraFlowDbContext> options) 
        : base(options)
    {
    }

    // Settings
    public DbSet<Settings> Settings => Set<Settings>();
    
    // Models and Packages
    public DbSet<PackageVersion> PackageVersions => Set<PackageVersion>();
    public DbSet<TrackedDownload> TrackedDownloads => Set<TrackedDownload>();
    
    // API related
    public DbSet<AuraCloud.AuraCloudAccount> AuraCloudAccounts => Set<AuraCloud.AuraCloudAccount>();
    public DbSet<CivitApi.CivitModel> CivitModels => Set<CivitApi.CivitModel>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        // Configure all entities from AuraFlow.Core
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(Settings).Assembly);
        
        // Global configuration
        modelBuilder.Entity<Settings>()
            .HasData(new Settings 
            { 
                Id = 1,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            });
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // Auto-update timestamps
        foreach (var entry in ChangeTracker.Entries())
        {
            if (entry.Entity is IHasTimestamps entity)
            {
                if (entry.State == EntityState.Added)
                    entity.CreatedAt = DateTime.UtcNow;
                
                entity.UpdatedAt = DateTime.UtcNow;
            }
        }

        return base.SaveChangesAsync(cancellationToken);
    }
}

/// <summary>
/// Interface for entities with timestamps
/// </summary>
public interface IHasTimestamps
{
    DateTime CreatedAt { get; set; }
    DateTime UpdatedAt { get; set; }
}
