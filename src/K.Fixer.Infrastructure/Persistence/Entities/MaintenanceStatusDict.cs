using K.Fixer.Infrastructure.Persistence.Base;

namespace K.Fixer.Infrastructure.Persistence.Entities;

public class MaintenanceStatusDict : BaseTranslatedDict
{
    public string Status { get; set; } = string.Empty;
}