namespace K.Fixer.Web.Api.Features.CompanyManage.Model;

public record UpdateCompanyBuildingRequest(
    string Name,
    string Street,
    string City,
    string PostalCode,
    string CountryCode);