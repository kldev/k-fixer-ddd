using K.Fixer.Domain.PropertyManagement.Aggregates.Building;

namespace K.Fixer.Domain.PropertyManagement.Repositories;

public interface ICompanyBuildingRepository
{
    Task AddAsync(Building companyBuilding, CancellationToken ct = default);

    Task<Building?> GetByIdAsync(Guid buildingId, CancellationToken ct = default);

    // Repozytorium nie ma metody Update — EF Core śledzi zmiany przez Change Tracker
    Task SaveChangesAsync(CancellationToken ct = default);
    
    Task<IReadOnlyList<Building>> GetBuildingsAsync(Guid companyId, string? querySearch,  int page,
        int pageSize, CancellationToken ct = default);
    Task<int> GetBuildingCountAsync(Guid companyId, string? querySearch, CancellationToken ct = default);
    
    Task<bool> IsNameUniqueAsync(
        string name, Guid companyId, Guid? excludeBuildingId, CancellationToken ct = default);
    
    public Task<Building?> GetByIdWithOccupanciesAsync(Guid buildingId, CancellationToken ct = default);
}