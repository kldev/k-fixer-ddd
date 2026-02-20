using Common.Toolkit.ResultPattern;

using K.Fixer.Application.Common;
using K.Fixer.Domain.Maintenance.Repositories;

namespace K.Fixer.Application.Maintenance.GetCompanyMaintenanceRequest;

public sealed class GetCompanyMaintenanceRequestHandler
{
    private readonly IMaintenanceRequestRepository _maintenanceRequestRepository;

    // ReSharper disable once ConvertToPrimaryConstructor
    public GetCompanyMaintenanceRequestHandler(IMaintenanceRequestRepository maintenanceRequestRepository)
    {
        _maintenanceRequestRepository = maintenanceRequestRepository;
    }

    public async Task<Result<PagedResult<MaintenanceRequestListItemDto>>> HandleAsync(GetCompanyMaintenanceRequestFilter filter,
        CancellationToken ct)
    {
        var items = await _maintenanceRequestRepository.GetPagedAsync(filter.QuerySearch, filter.Status, filter.Priority, filter.PageNumber, filter.PageSize, filter.CompanyId, null, ct);
        var total = await _maintenanceRequestRepository.CountAsync(filter.QuerySearch, filter.Status, filter.Priority, filter.CompanyId, null, ct);
        var buildingIds = items.Select(item => item.BuildingId);
        var assignedToIds = items.Select(item => item.AssignedToId).OfType<Guid>();
        var buildingNames = await _maintenanceRequestRepository.GetBuildingsByIdsAsync(buildingIds, ct);
        var residents = await _maintenanceRequestRepository.GetResidancesByIdsAsync(assignedToIds, ct);

        return new PagedResult<MaintenanceRequestListItemDto>(
            items.Select(m =>
            {
                var building   = buildingNames.FirstOrDefault(z => z.Id == m.BuildingId);
                var assignedTo = m.AssignedToId.HasValue
                    ? residents.FirstOrDefault(u => u.Id == m.AssignedToId.Value)
                    : null;

                return new MaintenanceRequestListItemDto(
                    m.Id,
                    m.Title.Value,
                    m.Status.Value,
                    m.Priority.Value,
                    building?.Name.Value ?? string.Empty,
                    assignedTo?.FullName.Value,
                    m.CreatedAt);
            }),
            total,
            filter.PageNumber,
            filter.PageSize);
    }
}
