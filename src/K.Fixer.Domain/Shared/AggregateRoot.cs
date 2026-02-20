namespace K.Fixer.Domain.Shared;

// Bazowa klasa dla Agregatów — rozszerza Entity o Domain Events
// Agregat zbiera zdarzenia w liście; infrastruktura je wysyła po zapisie
public abstract class AggregateRoot<TId> : Entity<TId>
{
    // long Id — tylko dla EF Core, niewidoczny poza infrastrukturą
    internal long RecordId { get; private set; }

    private readonly List<IDomainEvent> _domainEvents = [];

    // Tylko do odczytu z zewnątrz — infrastruktura czyta po zapisie do bazy
    public IReadOnlyCollection<IDomainEvent> DomainEvents
        => _domainEvents.AsReadOnly();

    protected AggregateRoot() { }
    protected AggregateRoot(TId id) : base(id) { }

    // Metoda chroniona — tylko agregat może dodawać swoje zdarzenia
    protected void RaiseDomainEvent(IDomainEvent domainEvent)
        => _domainEvents.Add(domainEvent);

    // Infrastruktura wywołuje tę metodę po wysłaniu zdarzeń
    public void ClearDomainEvents()
        => _domainEvents.Clear();
}