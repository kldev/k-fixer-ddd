using K.Fixer.Domain.Shared;

namespace K.Fixer.Domain.Maintenance.Events;

public sealed record RequestReopenedEvent(
    Guid RequestId,
    Guid ReopenedByCompanyAdminId,
    string Reason,
    DateTime OccurredAt
) : IDomainEvent;