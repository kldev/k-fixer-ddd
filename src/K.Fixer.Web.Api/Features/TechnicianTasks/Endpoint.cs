using System.Security.Claims;

using K.Fixer.Application.Common;
using K.Fixer.Application.Maintenance.CloseRequest;
using K.Fixer.Application.Maintenance.CompleteRequest;
using K.Fixer.Application.Maintenance.GetCompanyMaintenanceRequest;
using K.Fixer.Application.Maintenance.GetTechnicianRequests;
using K.Fixer.Application.Maintenance.StartWorkOnRequest;
using K.Fixer.Web.Api.Authorization;
using K.Fixer.Web.Api.Authorization.Extensions;
using K.Fixer.Web.Api.Extensions;
using K.Fixer.Web.Api.Features.TechnicianTasks.Model;

using Microsoft.AspNetCore.Mvc;

using GetTechnicianRequestsFilter = K.Fixer.Application.Maintenance.GetTechnicianRequests.GetTechnicianRequestsFilter;

namespace K.Fixer.Web.Api.Features.TechnicianTasks;


public static class Endpoint
{
    public static void Map(WebApplication app)
    {
        app.MapGroup("/api/technician/requests")
            .MapGroupEndpoints()
            .WithTags("Technician tasks")
            .RequireAuthorization(Policies.CanManageTechnicianTasks);
    }

    private static RouteGroupBuilder MapGroupEndpoints(this RouteGroupBuilder builder)
    {
        builder.MapGet("/", HandleGetRequests)
            .Produces<PagedResult<TechnicianRequestListItemDto>>();
        builder.MapPost("/{requestGid:guid}/start", HandleStartRequest);
        builder.MapPost("/{requestGid:guid}/complete", HandleCompleteRequest);
        return builder;
    }
    
    private static async Task<IResult> HandleGetRequests(ClaimsPrincipal cp,
        GetTechnicianRequestsHandler handler,
        string? querySearch, int? pageNumber, int? pageSize, string? status, string? priority,
        CancellationToken token)
    {
        var userId = cp.GetUserId();
        var filter = new GetTechnicianRequestsFilter(userId, status, pageNumber ?? 1, pageSize ?? 20);

        return (await handler.HandleAsync(filter, token)).ToApiResult();
    }

    private static async Task<IResult> HandleStartRequest(
        ClaimsPrincipal cp,
        StartWorkOnRequestHandler handler,
        Guid requestGid,
        CancellationToken token)
        => (await handler.HandleAsync(new StartWorkOnRequestCommand(cp.GetUserId(), requestGid), token)).ToApiResult();
    
    private static async Task<IResult> HandleCompleteRequest(
        ClaimsPrincipal cp,
        CompleteRequestHandler handler,
        Guid requestGid,
        [FromBody] CompleteRequestBody body,
        CancellationToken token)
        => (await handler.HandleAsync(new CompleteRequestCommand(cp.GetUserId(), requestGid, body.ResolutionNote), token)).ToApiResult();
}
