namespace K.Fixer.Application.Maintenance.StartWorkOnRequest;

public sealed record StartWorkOnRequestCommand(
    Guid TechnicianId,
    Guid MaintenanceRequestId);
