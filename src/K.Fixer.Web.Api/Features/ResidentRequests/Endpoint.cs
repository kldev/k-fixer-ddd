using System.Security.Claims;

using K.Fixer.Application.Maintenance.CreateMaintenanceRequest;
using K.Fixer.Application.Maintenance.UpdateMaintenanceRequest;
using K.Fixer.Web.Api.Authorization.Extensions;
using K.Fixer.Web.Api.Extensions;
using K.Fixer.Web.Api.Features.ResidentRequests.Model;

using Microsoft.AspNetCore.Mvc;

namespace K.Fixer.Web.Api.Features.ResidentRequests;


public  static class Endpoint
{
    public static void Map(WebApplication app)
    {
        app.MapGroup("resident-requests")
            .MapGroupEndpoints()
            .WithTags("Resident requests")
            .RequireAuthorization();
    }

    private static RouteGroupBuilder MapGroupEndpoints(this RouteGroupBuilder builder)
    {
        builder.MapPost("/", HandleCreateRequest);
        builder.MapPut("/{requestId:guid}", HandleUpdateRequest);
        
        return builder;
    }

    private static async Task<IResult> HandleCreateRequest(ClaimsPrincipal cp,
        CreateMaintenanceRequestHandler handler,
        [FromBody] ResidentMaintenanceRequest request, CancellationToken token)
    {
        var command = new CreateMaintenanceRequestCommand(cp.GetUserId(), null, request.Title, request.Description);
        return (await handler.HandleAsync(command, token)).ToApiResult();
    }

    private static async Task<IResult> HandleUpdateRequest(ClaimsPrincipal cp,
        UpdateMaintenanceRequestHandler handler,
        Guid requestId,
        [FromBody] ResidentMaintenanceRequest request, CancellationToken token)
    {
        var command =
            new UpdateMaintenanceRequestCommand(cp.GetUserId(), requestId, request.Title, request.Description);
        return (await handler.HandleAsync(command, token)).ToApiResult();
    }
}