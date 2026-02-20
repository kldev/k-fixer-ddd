using K.Fixer.Domain.PropertyManagement.Aggregates.Building;

using Microsoft.EntityFrameworkCore;

namespace K.Fixer.Infrastructure.Persistence.Extensions;

public static class BuildingExtensions
{
    public static IQueryable<Building> WithOccupancies(this IQueryable<Building> queryable)
        => queryable.Include(z => z.Occupancies);
    
    public static IQueryable<Building> FromCompanyId(this IQueryable<Building> queryable, Guid companyId)
        => queryable.Where(z=>z.CompanyId == companyId);
}