using K.Fixer.Domain.Shared;

namespace K.Fixer.Domain.Maintenance.Events;

public sealed record WorkStartedEvent(
    Guid RequestId,
    Guid TechnicianId,
    DateTime StartedAt
) : IDomainEvent;