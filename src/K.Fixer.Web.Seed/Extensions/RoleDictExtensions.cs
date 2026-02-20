using K.Fixer.Domain.IAM.Aggregates.User;
using K.Fixer.Infrastructure.Persistence.Entities;

namespace K.Fixer.Web.Seed.Extensions;

public static class RoleDictExtensions
{
    public static RoleDict ToEntity(this UserRole role)
    {
        return new RoleDict()
        {
            Id = role.NumericValue,
            Role = role.ToString(),
            NameEN = role.ToString(),
            NamePL = ""
        };
    }
}