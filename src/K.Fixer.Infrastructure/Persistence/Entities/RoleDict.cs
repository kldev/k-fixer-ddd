using K.Fixer.Infrastructure.Persistence.Base;

namespace K.Fixer.Infrastructure.Persistence.Entities;

public class RoleDict : BaseTranslatedDict
{
    public string Role { get; set; } = string.Empty;
}