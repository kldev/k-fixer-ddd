using K.Fixer.Domain.IAM.Aggregates.User;
using K.Fixer.Web.Api.Authorization.Requirements;

using Microsoft.AspNetCore.Authorization;

namespace K.Fixer.Web.Api.Authorization.Handlers;

public sealed class ManageCompanyHandler : AuthorizationHandler<ManageCompanyRequirement>
{
    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context,
      ManageCompanyRequirement requirement)
    {
        if (context.User.IsInRole(UserRole.Admin.Value))
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}