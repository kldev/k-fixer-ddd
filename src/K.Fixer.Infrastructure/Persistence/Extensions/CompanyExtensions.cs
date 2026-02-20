using K.Fixer.Domain.PropertyManagement.Aggregates.Company;

namespace K.Fixer.Infrastructure.Persistence.Extensions;

public static class CompanyExtensions
{
    public static IQueryable<Company> WithTaxId(this IQueryable<Company> query, TaxId taxId)
    {
        return query.Where(z => z.TaxId.CountryCode == taxId.CountryCode && z.TaxId.Number == taxId.Number);
    }
}