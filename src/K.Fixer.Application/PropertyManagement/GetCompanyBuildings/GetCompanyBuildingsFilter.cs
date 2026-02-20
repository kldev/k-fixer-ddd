namespace K.Fixer.Application.PropertyManagement.GetCompanyBuildings;

public sealed record GetCompanyBuildingsFilter
(Guid CompanyId, string? QuerySearch, int PageNumber, int PageSize);