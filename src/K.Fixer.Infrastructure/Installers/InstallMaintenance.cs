using K.Fixer.Application.Maintenance.AssignTechnician;
using K.Fixer.Application.Maintenance.CancelRequest;
using K.Fixer.Application.Maintenance.ChangeRequestPriority;
using K.Fixer.Application.Maintenance.CloseRequest;
using K.Fixer.Application.Maintenance.CompleteRequest;
using K.Fixer.Application.Maintenance.CreateMaintenanceRequest;
using K.Fixer.Application.Maintenance.GetCompanyMaintenanceRequest;
using K.Fixer.Application.Maintenance.GetTechnicianRequests;
using K.Fixer.Application.Maintenance.ReopenRequest;
using K.Fixer.Application.Maintenance.StartWorkOnRequest;
using K.Fixer.Application.Maintenance.UpdateMaintenanceRequest;
using K.Fixer.Domain.Maintenance.Repositories;
using K.Fixer.Domain.Maintenance.Services;
using K.Fixer.Infrastructure.Maintenance.Repositories;

using Microsoft.Extensions.DependencyInjection;

namespace K.Fixer.Infrastructure.Installers;

public static class InstallMaintenance
{
    public static IServiceCollection Install(this IServiceCollection services)
    {
        services.AddScoped<IMaintenanceRequestRepository, MaintenanceRequestRepository>();
        services.AddScoped<ResidentEligibilityService>();
        services.AddScoped<CreateMaintenanceRequestHandler>();
        services.AddScoped<UpdateMaintenanceRequestHandler>();
        services.AddScoped<GetCompanyMaintenanceRequestHandler>();
        services.AddScoped<AssignTechnicianHandler>();
        services.AddScoped<CancelRequestHandler>();
        services.AddScoped<ReopenRequestHandler>();
        services.AddScoped<CloseRequestHandler>();
        services.AddScoped<StartWorkOnRequestHandler>();
        services.AddScoped<ChangeRequestPriorityHandler>();
        services.AddScoped<CompleteRequestHandler>();
        services.AddScoped<GetTechnicianRequestsHandler>();
        
        return services;
    }
}