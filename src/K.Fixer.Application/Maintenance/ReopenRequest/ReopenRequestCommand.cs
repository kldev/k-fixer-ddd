namespace K.Fixer.Application.Maintenance.ReopenRequest;

public sealed record ReopenRequestCommand(
    Guid AdminId,
    Guid MaintenanceRequestId,
    string Reason);
