using K.Fixer.Domain.IAM.Aggregates.User;
using K.Fixer.Web.Api.Authorization.Requirements;

using Microsoft.AspNetCore.Authorization;

namespace K.Fixer.Web.Api.Authorization.Handlers;

public class CanManageTechnicianTasksHandler : AuthorizationHandler<CanManageTechnicianTasksRequirement>
{
    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context,
        CanManageTechnicianTasksRequirement requirement)
    {
        if (context.User.IsInRole(UserRole.Technician.Value))
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}
