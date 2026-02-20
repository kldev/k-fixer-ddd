using K.Fixer.Domain.Shared;

namespace K.Fixer.Domain.PropertyManagement.Events;

public sealed record CompanyRegisteredEvent(
    Guid     CompanyId,
    string   CompanyName,
    string   TaxId,           // np. "PL5260207427"
    DateTime OccurredAt
) : IDomainEvent;