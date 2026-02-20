namespace K.Fixer.Application.PropertyManagement.GetCompanyBuildings;

public sealed record CompanyBuildingListItemDto
(
    Guid Gid,
    string Name,
    string Address,
    bool IsActive,
    DateTime CreatedAt
);
