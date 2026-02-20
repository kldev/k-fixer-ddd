using K.Fixer.Domain.PropertyManagement.Aggregates.Building;
using K.Fixer.Domain.PropertyManagement.Repositories;
using K.Fixer.Infrastructure.Persistence;
using K.Fixer.Infrastructure.Persistence.Extensions;

using Microsoft.EntityFrameworkCore;

namespace K.Fixer.Infrastructure.PropertyManagement.Repositories;

public sealed class CompanyBuildingRepository : ICompanyBuildingRepository
{
    private readonly AppDbContext _dbContext;

    // ReSharper disable once ConvertToPrimaryConstructor
    public CompanyBuildingRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddAsync(Building companyBuilding, CancellationToken ct = default)
        => await _dbContext.AddAsync(companyBuilding, ct);

    public async Task<Building?> GetByIdAsync(Guid buildingId, CancellationToken ct = default)
        => await _dbContext.Buildings.FirstOrDefaultAsync(b => b.Id == buildingId, ct);

    public async Task SaveChangesAsync(CancellationToken ct = default)
        => await _dbContext.SaveChangesAsync(ct);
    
    public async Task<IReadOnlyList<Building>> GetBuildingsAsync(Guid companyId, string? querySearch, int page, int pageSize, CancellationToken ct = default)
    {
        var query = GetQueryableBuilding(querySearch);
        query = query.FromCompanyId(companyId);
        
        return await query
            .OrderBy(c => c.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);
    }

    public async Task<int> GetBuildingCountAsync(Guid companyId, string? querySearch, CancellationToken ct = default)
    {
        var query = GetQueryableBuilding(querySearch);
        query = query.FromCompanyId(companyId);
        return await query.CountAsync(ct);
    }
    
    private IQueryable<Building> GetQueryableBuilding(string? querySearch)
    {
        IQueryable<Building> query;

        if (!string.IsNullOrWhiteSpace(querySearch))
        {
            var pattern = $"%{querySearch}%";
            query = _dbContext.Buildings.FromSqlInterpolated(
                $"SELECT * FROM building WHERE name LIKE {pattern} OR address_city LIKE {pattern} OR address_street like '{pattern}'");
        }
        else
        {
            query = _dbContext.BuildingsReads;
        }

        return query;
    }

    public async Task<bool> IsNameUniqueAsync(string name, Guid companyId,Guid? excludeBuildingId, CancellationToken ct = default)
    {
        var queryable = _dbContext.BuildingsReads.AsQueryable().FromCompanyId(companyId);
        if (excludeBuildingId.HasValue)
        {
            queryable = queryable.Where(z => z.Id != excludeBuildingId.Value);
        }
        
        var exists = await queryable.AnyAsync(z => z.Name == BuildingName.Create(name).Value!, ct);
        return !exists;
    }

    public async Task<Building?> GetByIdWithOccupanciesAsync(Guid buildingId, CancellationToken ct = default)
    {
        return await _dbContext.BuildingsWithOccupanciesReads.FirstOrDefaultAsync(z => z.Id == buildingId, ct);
    }
}