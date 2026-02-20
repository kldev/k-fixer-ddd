using Common.Toolkit.ResultPattern;

using K.Fixer.Application.Common;
using K.Fixer.Domain.PropertyManagement.Repositories;

namespace K.Fixer.Application.PropertyManagement.GetCompanyBuildings;

public sealed class GetCompanyBuildingsHandler
{
    private readonly ICompanyBuildingRepository _buildingRepository;

    // ReSharper disable once ConvertToPrimaryConstructor
    public GetCompanyBuildingsHandler(ICompanyBuildingRepository buildingRepository)
    {
        _buildingRepository = buildingRepository;
    }

    public async Task<Result<PagedResult<CompanyBuildingListItemDto>>> HandleAsync(GetCompanyBuildingsFilter filter,
        CancellationToken ct = default)
    {
        var items = await _buildingRepository.GetBuildingsAsync(filter.CompanyId, filter.QuerySearch, filter.PageNumber, filter.PageSize,
            ct);
        var total = await _buildingRepository.GetBuildingCountAsync(filter.CompanyId, filter.QuerySearch, ct);

        return new PagedResult<CompanyBuildingListItemDto>(
            items.Select(c =>
                new CompanyBuildingListItemDto(c.Id, c.Name.Value, c.Address.ToString(), c.IsActive, c.CreatedAt)),
            total,
            filter.PageNumber,
            filter.PageSize);
    }
}