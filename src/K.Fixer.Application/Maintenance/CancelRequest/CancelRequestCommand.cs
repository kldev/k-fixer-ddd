namespace K.Fixer.Application.Maintenance.CancelRequest;

public sealed record CancelRequestCommand(
    Guid CancelByUserId,
    Guid MaintenanceRequestId,
    Guid AdminCompanyId);