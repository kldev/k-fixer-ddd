namespace K.Fixer.Application.Maintenance.AssignTechnician;

public sealed record AssignTechnicianCommand
(Guid AssignedBy, Guid TechnicianId, Guid MaintenanceRequestId, Guid AdminCompanyId);