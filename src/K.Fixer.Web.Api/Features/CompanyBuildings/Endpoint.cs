using System.Security.Claims;

using K.Fixer.Application.Common;
using K.Fixer.Application.PropertyManagement.CreateCompanyBuilding;
using K.Fixer.Application.PropertyManagement.GetCompanyBuildings;
using K.Fixer.Application.PropertyManagement.UpdateCompanyBuilding;
using K.Fixer.Web.Api.Authorization;
using K.Fixer.Web.Api.Authorization.Extensions;
using K.Fixer.Web.Api.Extensions;
using K.Fixer.Web.Api.Features.CompanyManage.Model;

using Microsoft.AspNetCore.Mvc;

namespace K.Fixer.Web.Api.Features.CompanyBuildings;

public static class Endpoint
{
    public static void Map(WebApplication app)
    {
        app.MapGroup("/api/company/building")
            .MapGroupEndpoints()
            .WithTags("Company buildings self-manage")
            .RequireAuthorization(Policies.CanManageOwnBuilding);
    }

    private static RouteGroupBuilder MapGroupEndpoints(this RouteGroupBuilder builder)
    {
        builder.MapGet("/", GetCompanyBuildings)
            .Produces<PagedResult<CompanyBuildingListItemDto>>();
        builder.MapPost("/", CreateCompanyBuilding)
            .Produces<CreateCompanyBuildingResult>();
        builder.MapPut("/{buildingGid:guid}", UpdateCompanyBuilding)
            .Produces<UpdateCompanyBuildingResult>();

        return builder;
    }

    private static async Task<IResult>
        GetCompanyBuildings(ClaimsPrincipal cp,
            GetCompanyBuildingsHandler handler,
            string? querySearch, int? pageNumber, int? pageSize,
            CancellationToken token)
    {
        var filter =
            new GetCompanyBuildingsFilter(cp.GetCompanyId()!.Value, querySearch, pageNumber ?? 1, pageSize ?? 20);
        return (await handler.HandleAsync(filter, token)).ToApiResult();
    }

    private static async Task<IResult> CreateCompanyBuilding(ClaimsPrincipal cp, CreateCompanyBuildingHandler handler,
        [FromBody] CreateCompanyBuildingRequest request, CancellationToken token)
    {
        var companyGid = cp.GetCompanyId()!.Value;
        var command = new CreateCompanyBuildingCommand(
            companyGid,
            request.Name,
            request.Street,
            request.City,
            request.PostalCode,
            request.CountryCode);

        return (await handler.HandleAsync(command, token)).ToApiResult();
    }
    
    private static async Task<IResult> UpdateCompanyBuilding(ClaimsPrincipal cp, UpdateCompanyBuildingHandler handler,
        [FromBody] UpdateCompanyBuildingRequest request, Guid buildingGid, CancellationToken token)
    {
        var companyGid = cp.GetCompanyId()!.Value;
        var command = new UpdateCompanyBuildingCommand(
            companyGid, 
    buildingGid,
            request.Name,
            request.Street,
            request.City,
            request.PostalCode,
            request.CountryCode);

        return (await handler.HandleAsync(command, token)).ToApiResult();
    }
}