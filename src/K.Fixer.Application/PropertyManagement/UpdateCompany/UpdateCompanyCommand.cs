namespace K.Fixer.Application.PropertyManagement.UpdateCompany;

public sealed record UpdateCompanyCommand(
    Guid CompanyId,
    string Name,
    string ContactEmail,
    string ContactPhone
);
