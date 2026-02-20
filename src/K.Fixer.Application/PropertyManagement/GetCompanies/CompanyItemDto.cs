namespace K.Fixer.Application.PropertyManagement.GetCompanies;

public sealed record CompanyListItemDto(
    Guid Gid,
    string Name,
    string TaxId,
    string? ContactEmail,
    string? ContactPhone,
    bool IsActive,
    DateTime CreatedAt
);