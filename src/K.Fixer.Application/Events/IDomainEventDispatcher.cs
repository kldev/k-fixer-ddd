using K.Fixer.Domain.Shared;

namespace K.Fixer.Application.Events;

public interface IDomainEventDispatcher
{
    Task DispatchAsync(
        IReadOnlyCollection<IDomainEvent> events,
        CancellationToken ct = default);
}