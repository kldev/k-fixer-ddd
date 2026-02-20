using K.Fixer.Domain.IAM.Aggregates.User;
using K.Fixer.Domain.Maintenance.Aggregates.MaintenanceRequest;
using K.Fixer.Infrastructure.Persistence;
using K.Fixer.Infrastructure.Persistence.Entities;
using K.Fixer.Web.Seed.Extensions;
using K.Fixer.Web.Seed.Extensions;

namespace K.Fixer.Web.Seed;


public class BaseDictSeeder
{
    private readonly AppDbContext _dbContext;

    // ReSharper disable once ConvertToPrimaryConstructor
    public BaseDictSeeder(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task SeedAsync()
    {
        await SeedRoles();
        await SeedMaintenancePriority();
        await SeedMaintenanceStatus();
    }

    private async Task SeedRoles()
    {
        var rolesDict = _dbContext.Set<RoleDict>();
        if (!rolesDict.Any())
        {
            rolesDict.Add(UserRole.Admin.ToEntity());
            rolesDict.Add(UserRole.CompanyAdmin.ToEntity());
            rolesDict.Add(UserRole.Resident.ToEntity());
            rolesDict.Add(UserRole.Technician.ToEntity());

            _dbContext.Set<RoleDict>().AddRange(rolesDict);
            await _dbContext.SaveChangesAsync();
        }
    }

    private async Task SeedMaintenancePriority()
    {
        var priorityDict = _dbContext.Set<MaintenancePriorityDict>();
        if (!priorityDict.Any())
        {
            priorityDict.Add(Priority.Low.ToEntity());
            priorityDict.Add(Priority.Medium.ToEntity());
            priorityDict.Add(Priority.High.ToEntity());
            priorityDict.Add(Priority.Urgent.ToEntity());

            _dbContext.Set<MaintenancePriorityDict>().AddRange(priorityDict);
            await _dbContext.SaveChangesAsync();
        }
    }

    private async Task SeedMaintenanceStatus()
    {
        var statusDict = _dbContext.Set<MaintenanceStatusDict>();
        if (!statusDict.Any())
        {
            statusDict.Add(MaintenanceStatus.New.ToEntity());
            statusDict.Add(MaintenanceStatus.Assigned.ToEntity());
            statusDict.Add(MaintenanceStatus.InProgress.ToEntity());
            statusDict.Add(MaintenanceStatus.Completed.ToEntity());
            statusDict.Add(MaintenanceStatus.Closed.ToEntity());
            statusDict.Add(MaintenanceStatus.Reopened.ToEntity());
            statusDict.Add(MaintenanceStatus.Cancelled.ToEntity());

            _dbContext.Set<MaintenanceStatusDict>().AddRange(statusDict);
            await _dbContext.SaveChangesAsync();
        }
    }
}