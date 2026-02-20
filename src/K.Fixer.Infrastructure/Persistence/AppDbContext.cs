using K.Fixer.Domain.IAM.Aggregates.AdminUser;
using K.Fixer.Domain.IAM.Aggregates.User;
using K.Fixer.Domain.Maintenance.Aggregates.MaintenanceRequest;
using K.Fixer.Domain.PropertyManagement.Aggregates.Building;
using K.Fixer.Domain.PropertyManagement.Aggregates.Company;
using K.Fixer.Infrastructure.Persistence.Entities;
using K.Fixer.Infrastructure.Persistence.Extensions;

using Microsoft.EntityFrameworkCore;

namespace K.Fixer.Infrastructure.Persistence;

public class AppDbContext : DbContext
{
    // ReSharper disable once ConvertToPrimaryConstructor
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }
    public DbSet<Company> Companies => Set<Company>(); 
    public IQueryable<Company> CompaniesReads => Set<Company>().AsNoTracking();
    
    public DbSet<User> Users => Set<User>();
    public IQueryable<User> UsersReads => Set<User>().AsNoTracking();
    public DbSet<AdminUser> AdminUsers => Set<AdminUser>();
    
    public DbSet<Building> Buildings => Set<Building>();
    public IQueryable<Building> BuildingsReads => Set<Building>()
        .AsNoTracking();
    
    public IQueryable<Building> BuildingsWithOccupanciesReads => Set<Building>()
        .WithOccupancies().AsNoTracking();
    
    // Requests
    public DbSet<MaintenanceRequest> Requests => Set<MaintenanceRequest>();
    public IQueryable<MaintenanceRequest> RequestsReads => Set<MaintenanceRequest>().AsNoTracking();
    
    public DbSet<StatusChangeLogEntry> StatusChanges => Set<StatusChangeLogEntry>();
    
    // public IQueryable<RoleDict> Roles => Set<RoleDict>().AsNoTracking();
    // public IQueryable<MaintenancePriorityDict> Priorities => Set<MaintenancePriorityDict>().AsNoTracking();
    // public IQueryable<MaintenanceStatusDict> Statuses => Set<MaintenanceStatusDict>().AsNoTracking();
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}