using K.Fixer.Domain.Maintenance.Aggregates.MaintenanceRequest;
using K.Fixer.Infrastructure.Persistence.Entities;

namespace K.Fixer.Web.Seed.Extensions;

public static class MaintenancePriorityExtensions
{
    public static MaintenancePriorityDict ToEntity(this Priority priority)
    {
        return new MaintenancePriorityDict()
        {
            Id = priority.NumericValue,
            Priority = priority.ToString(),
            NameEN = priority.ToString(),
            NamePL = ""
        };
    }
}