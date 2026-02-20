using K.Fixer.Web.Api.Authorization.Handlers;
using K.Fixer.Web.Api.Authorization.Requirements;

using Microsoft.AspNetCore.Authorization;

namespace K.Fixer.Web.Api.Authorization;

public static class AuthorizationExtensions
{
    public static IServiceCollection BuildPolicies(this IServiceCollection services)
    {
        services.AddAuthorizationBuilder()
            .AddPolicy(Policies.CanManageCompany, policy =>
                policy.AddRequirements(new ManageCompanyRequirement()))
            .AddPolicy(Policies.CanCreateBuilding, policy =>
                policy.AddRequirements(new CanCreateBuildingRequirement()))
            .AddPolicy(Policies.CanManageOwnBuilding, policy =>
                policy.AddRequirements(new CanCreateCompanyOwnBuildingRequirement()))
            .AddPolicy(Policies.CanCreateMaintenanceRequest, policy =>
                policy.AddRequirements(new CanCreateMaintenanceRequestRequirement()))
            .AddPolicy(Policies.CanManageCompanyRequests, policy =>
                policy.AddRequirements(new CanManageCompanyRequestsRequirement()))
            .AddPolicy(Policies.CanManageTechnicianTasks, policy =>
                policy.AddRequirements(new CanManageTechnicianTasksRequirement()));

        services.AddSingleton<IAuthorizationHandler, ManageCompanyHandler>();
        services.AddSingleton<IAuthorizationHandler, CanCreateBuildingHandler>();
        services.AddSingleton<IAuthorizationHandler, CanCreateCompanyOwnBuildingHandler>();

        services.AddSingleton<IAuthorizationHandler, CanCreateMaintenanceRequestHandler>();
        services.AddSingleton<IAuthorizationHandler, CanManageCompanyRequestsHandler>();
        services.AddSingleton<IAuthorizationHandler, CanManageTechnicianTasksHandler>();

        return services;
    }
}