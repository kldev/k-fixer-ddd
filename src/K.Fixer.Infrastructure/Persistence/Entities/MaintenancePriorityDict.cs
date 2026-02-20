using K.Fixer.Infrastructure.Persistence.Base;

namespace K.Fixer.Infrastructure.Persistence.Entities;

public class MaintenancePriorityDict : BaseTranslatedDict
{
    public string Priority { get; set; } = string.Empty;
}