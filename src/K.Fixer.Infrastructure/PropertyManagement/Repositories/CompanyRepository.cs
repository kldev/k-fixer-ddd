using K.Fixer.Domain.PropertyManagement.Aggregates.Company;
using K.Fixer.Domain.PropertyManagement.Repositories;
using K.Fixer.Infrastructure.Persistence;
using K.Fixer.Infrastructure.Persistence.Extensions;

using Microsoft.EntityFrameworkCore;

namespace K.Fixer.Infrastructure.PropertyManagement.Repositories;

public class CompanyRepository : ICompanyRepository
{
    private readonly AppDbContext _dbContext;

    // ReSharper disable once ConvertToPrimaryConstructor
    public CompanyRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Company?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await _dbContext.CompaniesReads.FirstOrDefaultAsync(z => z.Id == id, ct);
    }

    public async Task<bool> IsNameUniqueAsync(string name, Guid? excludeId = null, CancellationToken ct = default)
    {
        var queryable = _dbContext.CompaniesReads.AsQueryable();
        if (excludeId.HasValue)
        {
            queryable = queryable.Where(z => z.Id != excludeId.Value);
        }

        var exists = await queryable.AnyAsync(z => z.Name == CompanyName.Create(name).Value!, ct);
        return !exists;
    }

    public async Task<bool> IsTaxIdUniqueAsync(string countryCode, string number, CancellationToken ct = default)
    {
        var taxId = TaxId.Create(countryCode, number).Value!;
        var exists = await _dbContext.CompaniesReads.WithTaxId(taxId)
            .AnyAsync( ct);
        return !exists;
    }

    public async Task AddAsync(Company company, CancellationToken ct = default) =>
        await _dbContext.Companies.AddAsync(company, ct);


    public async Task SaveChangesAsync(CancellationToken ct = default) =>
        await _dbContext.SaveChangesAsync(ct);


    public async Task<IReadOnlyList<Company>> GetPagedAsync(string? querySearch, int page, int pageSize,
        CancellationToken ct = default)
    {
        var query = GetQueryableCompany(querySearch);

        return await query
            .OrderBy(c => c.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);
    }

    private IQueryable<Company> GetQueryableCompany(string? querySearch)
    {
        IQueryable<Company> query;

        if (!string.IsNullOrWhiteSpace(querySearch))
        {
            query = _dbContext.Companies.Where(z =>
                z.Name.CompanyNameInnerValue().Contains(querySearch) 
                || z.ContactEmail.ContactEmailInnerValue().Contains(querySearch) ||
                z.ContactPhone.ContactPhoneInnerValue().Contains(querySearch)
                );
            //   || z.TaxId.Value.Contains(querySearch));
        }
        else
        {
            query = _dbContext.CompaniesReads;
        }

        return query;
    }

    public async Task<int> CountAsync(string? querySearch, CancellationToken ct = default)
    {
        var query = GetQueryableCompany(querySearch);
        return await query.CountAsync(ct);
    }
}