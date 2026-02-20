using K.Fixer.Domain.IAM.Aggregates.User;
using K.Fixer.Domain.Maintenance.Aggregates.MaintenanceRequest;
using K.Fixer.Domain.PropertyManagement.Aggregates.Building;

namespace K.Fixer.Domain.Maintenance.Repositories;

public interface IMaintenanceRequestRepository
{
    Task<MaintenanceRequest?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task AddAsync(MaintenanceRequest request, CancellationToken ct = default);
    Task SaveChangesAsync(CancellationToken ct = default);
    Task<Guid?> GetResidentActiveBuildingAsync(Guid residentId, CancellationToken ct = default);
    Task<IReadOnlyList<MaintenanceRequest>> GetPagedAsync(string? querySearch, string? filterStatus, string? filterPriority, int filterPageNumber, int filterPageSize, Guid? companyId, Guid? technicianId, CancellationToken ct);
    Task<int> CountAsync(string? querySearch, string? filterStatus, string? filterPriority, Guid? companyId, Guid? technicianId, CancellationToken ct);
    
    public Task<IReadOnlyCollection<Building>> GetBuildingsByIdsAsync(IEnumerable<Guid> ids, CancellationToken ct = default);
    Task<IReadOnlyCollection<User>> GetResidancesByIdsAsync(IEnumerable<Guid> usersIds, CancellationToken ct);
    Task AddLogsAsync(IReadOnlyCollection<StatusChangeLogEntry> requestStatusLog);
}