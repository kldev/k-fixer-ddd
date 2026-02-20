using K.Fixer.Domain.Shared;

namespace K.Fixer.Domain.Maintenance.Events;

public sealed record WorkCompletedEvent(
    Guid RequestId,
    Guid TechnicianId,
    string ResolutionNote,
    DateTime CompletedAt
) : IDomainEvent;