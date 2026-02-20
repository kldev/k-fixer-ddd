namespace K.Fixer.Application.Maintenance.ChangeRequestPriority;

public sealed record ChangeRequestPriorityCommand(
    Guid ChangedById,
    Guid MaintenanceRequestId,
    string NewPriority,
    Guid AdminCompanyId);
