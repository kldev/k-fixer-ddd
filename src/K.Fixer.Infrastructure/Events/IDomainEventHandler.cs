using K.Fixer.Domain.Shared;

namespace K.Fixer.Infrastructure.Events;

public interface IDomainEventHandler<TEvent> where TEvent : IDomainEvent
{
    Task HandleAsync(TEvent domainEvent, CancellationToken ct = default);
}