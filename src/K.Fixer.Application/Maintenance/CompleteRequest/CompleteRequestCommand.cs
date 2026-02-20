namespace K.Fixer.Application.Maintenance.CompleteRequest;

public sealed record CompleteRequestCommand(Guid TechnicianId, Guid MaintenanceRequestId, string ResolutionNote);
