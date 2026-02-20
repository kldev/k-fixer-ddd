namespace K.Fixer.Application.PropertyManagement.CreateCompany;

public sealed record CreateCompanyCommand(
    string Name,
    string TaxIdCountryCode,   // np. "PL", "DE"
    string TaxIdNumber,        // np. "5260207427"
    string ContactEmail,
    string ContactPhone
);