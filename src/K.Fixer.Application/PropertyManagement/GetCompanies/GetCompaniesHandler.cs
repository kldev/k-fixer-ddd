using System.Security.Claims;

using Common.Toolkit.ResultPattern;

using K.Fixer.Application.Common;
using K.Fixer.Domain.PropertyManagement.Repositories;

namespace K.Fixer.Application.PropertyManagement.GetCompanies;

public sealed class GetCompaniesHandler
{
    private readonly ICompanyRepository _companyRepository;

    // ReSharper disable once ConvertToPrimaryConstructor
    public GetCompaniesHandler(ICompanyRepository companyRepository)
    {
        _companyRepository = companyRepository;
    }

    public async Task<Result<PagedResult<CompanyListItemDto>>> HandleAsync(
        GetCompanyFilter filter, CancellationToken ct)
    {
        var items = await _companyRepository.GetPagedAsync(filter.QuerySearch, filter.PageNumber, filter.PageSize, ct);
        var total = await _companyRepository.CountAsync(filter.QuerySearch, ct);

        return new PagedResult<CompanyListItemDto>(
            items.Select(c => new CompanyListItemDto(c.Id, c.Name.Value, c.TaxId.Formatted!, c.ContactEmail.Value,
                c.ContactPhone.Value ?? "", c.IsActive, c.CreatedAt)),
            total,
            filter.PageNumber,
            filter.PageSize);
    }
}