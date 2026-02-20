namespace K.Fixer.Application.Maintenance.GetTechnicianRequests;

public record TechnicianRequestListItemDto(
    Guid Gid,
    string Title,
    string Status,
    string Priority,
    string BuildingName,
    DateTime CreatedAt,
    DateTime? StartDateAtUtc
);
