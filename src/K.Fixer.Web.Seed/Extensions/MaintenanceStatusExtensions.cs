using K.Fixer.Domain.Maintenance.Aggregates.MaintenanceRequest;
using K.Fixer.Infrastructure.Persistence.Entities;

namespace K.Fixer.Web.Seed.Extensions;

public static class MaintenanceStatusExtensions
{
    public static MaintenanceStatusDict ToEntity(this MaintenanceStatus status)
    {
        return new MaintenanceStatusDict()
        {
            Id = status.NumericValue,
            Status = status.ToString(),
            NameEN = status.ToString(),
            NamePL = ""
        };
    }
}