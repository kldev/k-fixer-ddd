using Common.Toolkit.ResultPattern;

using K.Fixer.Application.Common;
using K.Fixer.Domain.Maintenance.Repositories;

namespace K.Fixer.Application.Maintenance.GetTechnicianRequests;

public sealed class GetTechnicianRequestsHandler
{
    private readonly IMaintenanceRequestRepository _maintenanceRequestRepository;

    // ReSharper disable once ConvertToPrimaryConstructor
    public GetTechnicianRequestsHandler(IMaintenanceRequestRepository maintenanceRequestRepository)
    {
        _maintenanceRequestRepository = maintenanceRequestRepository;
    }

    public async Task<Result<PagedResult<TechnicianRequestListItemDto>>> HandleAsync(GetTechnicianRequestsFilter filter,
        CancellationToken ct)
    {
        var items = await _maintenanceRequestRepository.GetPagedAsync(null, filter.Status, null, filter.PageNumber,
            filter.PageSize, null, filter.TechnicianId, ct);
        var total = await _maintenanceRequestRepository.CountAsync(null, filter.Status, null, null, filter.TechnicianId, ct);
        var buildingIds = items.Select(item => item.BuildingId);
        var buildingNames = await _maintenanceRequestRepository.GetBuildingsByIdsAsync(buildingIds, ct);

        return new PagedResult<TechnicianRequestListItemDto>(
            items.Select(m =>
            {
                var building = buildingNames.FirstOrDefault(z => z.Id == m.BuildingId);
                return new TechnicianRequestListItemDto(
                    m.Id,
                    m.Title.Value,
                    m.Status.Value,
                    m.Priority.Value,
                    building?.Name.Value ?? string.Empty,
                    m.CreatedAt, m.WorkPeriod?.StartedAt);
            }),
            total,
            filter.PageNumber,
            filter.PageSize);
    }
}