using K.Fixer.Domain.Shared;

namespace K.Fixer.Domain.PropertyManagement.Events;

public sealed record CompanyDeactivatedEvent(
    Guid     CompanyId,
    string   CompanyName,
    Guid     DeactivatedByAdminId,
    DateTime OccurredAt
) : IDomainEvent;