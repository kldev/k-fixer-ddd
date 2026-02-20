namespace K.Fixer.Application.PropertyManagement.CreateCompanyBuilding;

public sealed record CreateCompanyBuildingCommand(
    Guid CompanyId,
    string Name,
    string Street,
    string City,
    string PostalCode,
    string CountryCode
);
