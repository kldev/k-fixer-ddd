using K.Fixer.Domain.IAM.Aggregates.User;
using K.Fixer.Web.Api.Authorization.Requirements;

using Microsoft.AspNetCore.Authorization;

namespace K.Fixer.Web.Api.Authorization.Handlers;

public class CanCreateBuildingHandler : AuthorizationHandler<CanCreateBuildingRequirement>
{
    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, CanCreateBuildingRequirement requirement)
    {
        if (context.User.IsInRole(UserRole.Admin.Value))
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}