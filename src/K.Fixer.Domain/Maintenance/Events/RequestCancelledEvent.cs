using K.Fixer.Domain.Shared;

namespace K.Fixer.Domain.Maintenance.Events;

public sealed record RequestCancelledEvent(
    Guid RequestId,
    Guid CancelledByUserId,
    string UserRole,
    DateTime OccurredAt
) : IDomainEvent;