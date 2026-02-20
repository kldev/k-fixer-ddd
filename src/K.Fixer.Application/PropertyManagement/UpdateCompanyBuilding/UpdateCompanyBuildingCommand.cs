namespace K.Fixer.Application.PropertyManagement.UpdateCompanyBuilding;

public sealed record UpdateCompanyBuildingCommand(
    Guid CompanyId,
    Guid BuildingId,
    string Name,
    string Street,
    string City,
    string PostalCode,
    string CountryCode
);
