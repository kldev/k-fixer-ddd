namespace K.Fixer.Web.Api.Features.CompanyManage.Model;

public record CreateCompanyBuildingRequest(
    string Name,
    string Street,
    string City,
    string PostalCode,
    string CountryCode);