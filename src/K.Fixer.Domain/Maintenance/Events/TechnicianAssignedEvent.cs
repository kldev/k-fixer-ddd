using K.Fixer.Domain.Shared;

namespace K.Fixer.Domain.Maintenance.Events;

public sealed record TechnicianAssignedEvent(
    Guid RequestId,
    Guid TechnicianId,
    Guid AssignedByCompanyAdminId,
    DateTime OccurredAt
) : IDomainEvent;