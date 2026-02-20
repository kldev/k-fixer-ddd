namespace K.Fixer.Application.Maintenance.UpdateMaintenanceRequest;

public sealed record UpdateMaintenanceRequestCommand
(
    Guid   ResidentId,
    Guid   RequestId, 
    string Title,
    string Description
);