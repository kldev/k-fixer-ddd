using K.Fixer.Domain.IAM.Aggregates.User;
using K.Fixer.Domain.Maintenance.Aggregates.MaintenanceRequest;
using K.Fixer.Domain.Maintenance.Repositories;
using K.Fixer.Domain.PropertyManagement.Aggregates.Building;
using K.Fixer.Infrastructure.Persistence;

using Microsoft.EntityFrameworkCore;

namespace K.Fixer.Infrastructure.Maintenance.Repositories;

public sealed class MaintenanceRequestRepository : IMaintenanceRequestRepository
{
    private readonly AppDbContext  _dbContext;

    // ReSharper disable once ConvertToPrimaryConstructor
    public MaintenanceRequestRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<MaintenanceRequest?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await _dbContext.Requests.FirstOrDefaultAsync(z => z.Id == id, ct);
    }

    public async Task AddAsync(MaintenanceRequest request, CancellationToken ct = default)
        => await _dbContext.Requests.AddAsync(request, ct);

    public async Task SaveChangesAsync(CancellationToken ct = default)
        => await _dbContext.SaveChangesAsync(ct);

    public async Task<Guid?> GetResidentActiveBuildingAsync(Guid residentId, CancellationToken ct = default)
    {
        var residentOccupancy = await _dbContext.Set<ResidentOccupancy>()
            .Where(z => z.ResidentId == residentId && z.Period.To == null)
            .OrderByDescending(z => z.Period.From).FirstOrDefaultAsync(ct);

        return residentOccupancy?.BuildingId ?? null;
    }

    public async Task<IReadOnlyList<MaintenanceRequest>> GetPagedAsync(
        string? querySearch,
        string? filterStatus,
        string? filterPriority,
        int filterPageNumber,
        int filterPageSize,
        Guid? companyId,
        Guid? technicianId,
        CancellationToken ct)
    {
        var query = GetQueryable(querySearch);

        var companyBuildingIdsQuery = _dbContext.BuildingsReads;
           // .Where(b => b.CompanyId == companyId)
           // .Select(b => b.Id);

        if (companyId != null)
        {
            companyBuildingIdsQuery = companyBuildingIdsQuery.Where(z => z.CompanyId == companyId);
            var companyBuildingIds = companyBuildingIdsQuery.Select(b => b.Id);
            query = query.Where(r => companyBuildingIds.Contains(r.BuildingId));

        }
        
        if (!string.IsNullOrWhiteSpace(filterStatus))
        {
            var statusResult = MaintenanceStatus.Create(filterStatus);
            if (!statusResult.IsFailure)
                query = query.Where(r => r.Status == statusResult.Value!);
        }

        if (!string.IsNullOrWhiteSpace(filterPriority))
        {
            var priorityResult = Priority.Create(filterPriority);
            if (!priorityResult.IsFailure)
                query = query.Where(r => r.Priority == priorityResult.Value!);
        }

        if (technicianId != null)
        {
            query = query.Where(z => z.AssignedToId == technicianId);
        }

        return await query
            .OrderByDescending(r => r.CreatedAt)
            .Skip((filterPageNumber - 1) * filterPageSize)
            .Take(filterPageSize)
            .ToListAsync(ct);
    }

    private IQueryable<MaintenanceRequest> GetQueryable(string? querySearch)
    {
        if (!string.IsNullOrWhiteSpace(querySearch))
        {
            var pattern = $"%{querySearch}%";
            
            return _dbContext.Requests.FromSqlInterpolated($"select * from maintenance_requests where title LIKE {pattern}"); 
                
        }
        return _dbContext.RequestsReads.Where(r => !r.IsDeleted);
    }

    public async Task<int> CountAsync(string? querySearch,
        string? filterStatus,
        string? filterPriority,
        Guid? companyId,
        Guid? technicianId, CancellationToken ct)
    {
        var query = GetQueryable(querySearch);
        var companyBuildingIdsQuery = _dbContext.BuildingsReads;
        
        if (companyId != null)
        {
            companyBuildingIdsQuery = companyBuildingIdsQuery.Where(z => z.CompanyId == companyId);
            var companyBuildingIds = companyBuildingIdsQuery.Select(b => b.Id);
            query = query.Where(r => companyBuildingIds.Contains(r.BuildingId));

        }
        
        if (!string.IsNullOrWhiteSpace(filterStatus))
        {
            var statusResult = MaintenanceStatus.Create(filterStatus);
            if (!statusResult.IsFailure)
                query = query.Where(r => r.Status == statusResult.Value!);
        }

        if (!string.IsNullOrWhiteSpace(filterPriority))
        {
            var priorityResult = Priority.Create(filterPriority);
            if (!priorityResult.IsFailure)
                query = query.Where(r => r.Priority == priorityResult.Value!);
        }

        if (technicianId != null)
        {
            query = query.Where(z => z.AssignedToId == technicianId);
        }
        
        return await query.CountAsync(ct);
    }

    public async Task<IReadOnlyCollection<Building>> GetBuildingsByIdsAsync(IEnumerable<Guid> ids, CancellationToken ct = default)
    {
        var idList = ids.Distinct().ToList();
        return await _dbContext.BuildingsReads
            .Where(b => idList.Contains(b.Id))
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyCollection<User>> GetResidancesByIdsAsync(IEnumerable<Guid> usersIds, CancellationToken ct)
    {
        var idList = usersIds.Distinct().ToList();
        return await _dbContext.UsersReads
            .Where(u => idList.Contains(u.Id))
            .ToListAsync(ct);
    }

    public async Task AddLogsAsync(IReadOnlyCollection<StatusChangeLogEntry> requestStatusLog)
    => await _dbContext.StatusChanges.AddRangeAsync(requestStatusLog);
}
