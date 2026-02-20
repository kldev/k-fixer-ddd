using K.Fixer.Domain.Shared;

namespace K.Fixer.Domain.Maintenance.Events;

public sealed record MaintenanceRequestCreatedEvent(
    Guid RequestId,
    Guid BuildingId,
    Guid CreatedByResidentId,
    string Title,
    DateTime OccurredAt
) : IDomainEvent;