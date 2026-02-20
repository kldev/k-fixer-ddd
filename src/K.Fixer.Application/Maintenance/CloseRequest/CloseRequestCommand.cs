namespace K.Fixer.Application.Maintenance.CloseRequest;

public sealed record CloseRequestCommand(
    Guid AdminId,
    Guid MaintenanceRequestId);
