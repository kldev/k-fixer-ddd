using K.Fixer.Domain.IAM.Aggregates.User;
using K.Fixer.Web.Api.Authorization.Extensions;
using K.Fixer.Web.Api.Authorization.Requirements;

using Microsoft.AspNetCore.Authorization;

namespace K.Fixer.Web.Api.Authorization.Handlers;

public class CanCreateCompanyOwnBuildingHandler : AuthorizationHandler<CanCreateCompanyOwnBuildingRequirement>
{
    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context,
      CanCreateCompanyOwnBuildingRequirement requirement)
    {
        if (context.User.IsInRole(UserRole.CompanyAdmin.Value))
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;

    }
}