namespace K.Fixer.Application.Maintenance.CreateMaintenanceRequest;

public sealed record CreateMaintenanceRequestCommand(
    Guid   ResidentId,
    Guid?   BuildingId, // or find active building for user
    string Title,
    string Description
);