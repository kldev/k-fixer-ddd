using K.Fixer.Domain.IAM.Aggregates.User;
using K.Fixer.Web.Api.Authorization.Requirements;

using Microsoft.AspNetCore.Authorization;

namespace K.Fixer.Web.Api.Authorization.Handlers;

public class CanCreateMaintenanceRequestHandler : AuthorizationHandler<CanCreateMaintenanceRequestRequirement>
{
    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, CanCreateMaintenanceRequestRequirement requirement)
    {
        if (context.User.IsInRole(UserRole.Resident.Value))
        {
            context.Succeed(requirement);
        }
        
        return Task.CompletedTask;
    }
}