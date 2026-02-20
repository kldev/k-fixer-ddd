using K.Fixer.Domain.Shared;

namespace K.Fixer.Domain.Maintenance.Events;

public sealed record RequestClosedEvent(
    Guid RequestId,
    Guid ClosedByCompanyAdminId,
    DateTime OccurredAt
) : IDomainEvent;