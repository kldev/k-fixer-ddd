using K.Fixer.Application.Common;
using K.Fixer.Application.PropertyManagement.CreateCompany;
using K.Fixer.Application.PropertyManagement.CreateCompanyBuilding;
using K.Fixer.Application.PropertyManagement.GetCompanies;
using K.Fixer.Application.PropertyManagement.GetCompanyBuildings;
using K.Fixer.Application.PropertyManagement.UpdateCompany;
using K.Fixer.Application.PropertyManagement.UpdateCompanyBuilding;
using K.Fixer.Web.Api.Authorization;
using K.Fixer.Web.Api.Extensions;
using K.Fixer.Web.Api.Features.CompanyManage.Model;

using Microsoft.AspNetCore.Mvc;

namespace K.Fixer.Web.Api.Features.CompanyManage;

public static class Endpoint
{
    public static void Map(WebApplication app)
    {
        app.MapGroup("/api/company-manage")
            .MapGroupEndpoints().WithTags("Owner company manage")
            .RequireAuthorization(Policies.CanManageCompany);
    }

    private static RouteGroupBuilder MapGroupEndpoints(this RouteGroupBuilder builder)
    {
        builder.MapGet("/", GetCompanies)
            .Produces<PagedResult<CompanyListItemDto>>();
        builder.MapPost("/", CreateCompany)
            .Produces<CreateCompanyResult>();
        builder.MapPut("/{companyGid:guid}", UpdateCompany);
        
        // buildings
        builder.MapGet("/{companyGid:guid}/building", GetBuildings)
            .Produces<PagedResult<CompanyBuildingListItemDto>>();
        builder.MapPost("/{companyGid:guid}/building", CreateBuilding)
            .Produces<CreateCompanyBuildingResult>();
        builder.MapPut("/{companyGid:guid}/building/{buildingGid:guid}", UpdateBuilding);

        return builder;
    }

    private static async Task<IResult> CreateCompany(
        CreateCompanyHandler handler,       // inject bezpośrednio
        [FromBody] CreateCompanyRequest request,
        CancellationToken token)
    {
        var command = new CreateCompanyCommand(
            request.Name, 
            "PL",
            request.TaxId.Replace("PL",""),
            request.ContactEmail ?? "",
            request.ContactPhone ?? "");

        return (await handler.HandleAsync(command, token)).ToApiResult();
    }
    
    private static async Task<IResult> UpdateCompany(
        UpdateCompanyHandler handler, 
        Guid companyGid,
        [FromBody] UpdateCompanyRequest request,
        CancellationToken token)
    {
        var command = new UpdateCompanyCommand(
            companyGid, 
            request.TaxId.Replace("PL",""),
            request.ContactEmail ?? "",
            request.ContactPhone ?? "");

        return (await handler.HandleAsync(command, token)).ToApiResult();
    }
    
    private static async Task<IResult> GetCompanies(
        GetCompaniesHandler handler, 
        string? querySearch, int? pageNumber, int? pageSize,
        CancellationToken token)
    {
        var query = new GetCompanyFilter(querySearch, pageNumber ?? 1 , pageSize ?? 20);

        return (await handler.HandleAsync(query, token)).ToApiResult();
    }

    private static async Task<IResult> GetBuildings(
        GetCompanyBuildingsHandler handler,  Guid companyGid,
        string? querySearch, int? pageNumber, int? pageSize,
        CancellationToken token)
    {
        var query = new GetCompanyBuildingsFilter(companyGid, querySearch, pageNumber ?? 1 , pageSize ?? 20);

        return (await handler.HandleAsync(query, token)).ToApiResult();
    }

    private static async Task<IResult> CreateBuilding(
        CreateCompanyBuildingHandler handler,
        Guid companyGid,
        [FromBody] CreateCompanyBuildingRequest request,
        CancellationToken token)
    {
        var command = new CreateCompanyBuildingCommand(
            companyGid,
            request.Name,
            request.Street,
            request.City,
            request.PostalCode,
            request.CountryCode);

        return (await handler.HandleAsync(command, token)).ToApiResult();
    }

    private static async Task<IResult> UpdateBuilding(
        UpdateCompanyBuildingHandler handler,
        Guid companyGid,
        Guid buildingGid,
        [FromBody] UpdateCompanyBuildingRequest request,
        CancellationToken token)
    {
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