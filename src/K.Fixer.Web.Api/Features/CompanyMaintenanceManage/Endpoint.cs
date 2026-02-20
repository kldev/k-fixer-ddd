using System.Security.Claims;

using K.Fixer.Application.Common;
using K.Fixer.Application.Maintenance.AssignTechnician;
using K.Fixer.Application.Maintenance.CancelRequest;
using K.Fixer.Application.Maintenance.ChangeRequestPriority;
using K.Fixer.Application.Maintenance.CloseRequest;
using K.Fixer.Application.Maintenance.GetCompanyMaintenanceRequest;
using K.Fixer.Web.Api.Authorization;
using K.Fixer.Web.Api.Authorization.Extensions;
using K.Fixer.Web.Api.Extensions;
using K.Fixer.Web.Api.Features.CompanyMaintenanceManage.Models;

using Microsoft.AspNetCore.Mvc;

namespace K.Fixer.Web.Api.Features.CompanyMaintenanceManage;

public static class Endpoint
{
    public static void Map(WebApplication app)
    {
        app.MapGroup("/api/company/requests")
            .MapGroupEndpoints()
            .WithTags("Company maintenance manage")
            .RequireAuthorization(Policies.CanManageCompanyRequests);
    }
    
    private static RouteGroupBuilder MapGroupEndpoints(this RouteGroupBuilder builder)
    {
        builder.MapGet("/", HandleGetRequests).Produces<PagedResult<MaintenanceRequestListItemDto>>();
        builder.MapPut("/{requestGid:guid}/assign", HandleAssignTechnician);
        builder.MapPatch("/{requestGid:guid}/priority", HandleChangePriority);
        builder.MapPost("/{requestGid:guid}/cancel", HandleCancelRequest);
        builder.MapPost("/{requestGid:guid}/close", HandleCloseRequest);

        return builder;
    }

    private static async Task<IResult> HandleGetRequests(ClaimsPrincipal cp,
        GetCompanyMaintenanceRequestHandler handler,
        string? querySearch, int? pageNumber, int? pageSize, string? status, string? priority,
        CancellationToken token)
    {
        var companyId = cp.GetCompanyId() ?? Guid.Empty;
        var filter = new GetCompanyMaintenanceRequestFilter(querySearch, status, priority, pageNumber ?? 1,
            pageSize ?? 20, companyId);

        return (await handler.HandleAsync(filter, token)).ToApiResult();
    }
    

    private static async Task<IResult> HandleAssignTechnician(
        ClaimsPrincipal cp,
        AssignTechnicianHandler handler,
        Guid requestGid,
        [FromBody] AssignTechnicianRequest request,
        CancellationToken token)
    {
        var command = new AssignTechnicianCommand(cp.GetUserId(), request.TechnicianGid, requestGid, cp.GetCompanyId() ?? Guid.Empty);
        return (await handler.HandleAsync(command, token)).ToApiResult();
    }

    private static async Task<IResult> HandleChangePriority(
        ClaimsPrincipal cp,
        ChangeRequestPriorityHandler handler,
        Guid requestGid,
        [FromBody] ChangePriorityRequest request,
        CancellationToken token)
    {
        var command = new ChangeRequestPriorityCommand(cp.GetUserId(), requestGid, request.Priority, cp.GetCompanyId() ?? Guid.Empty);
        return (await handler.HandleAsync(command, token)).ToApiResult();
    } 

    private static async Task<IResult> HandleCancelRequest(
        ClaimsPrincipal cp,
        CancelRequestHandler handler,
        Guid requestGid,
        CancellationToken token)
    => (await handler.HandleAsync(new CancelRequestCommand(cp.GetUserId(), requestGid, cp.GetCompanyId() ?? Guid.Empty), token)).ToApiResult();
    
    
    private static async Task<IResult> HandleCloseRequest(
        ClaimsPrincipal cp,
        CloseRequestHandler handler,
        Guid requestGid,
        CancellationToken token)
        => (await handler.HandleAsync(new CloseRequestCommand(cp.GetUserId(), requestGid), token)).ToApiResult();
}