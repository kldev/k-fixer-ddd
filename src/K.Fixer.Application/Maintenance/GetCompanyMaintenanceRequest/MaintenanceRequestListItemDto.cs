namespace K.Fixer.Application.Maintenance.GetCompanyMaintenanceRequest;

public sealed record MaintenanceRequestListItemDto(
    Guid Id,
    string Title,
    string Status,
    string Priority,
    string BuildingName,
    string? AssignedToName,
    DateTime CreatedAt
);