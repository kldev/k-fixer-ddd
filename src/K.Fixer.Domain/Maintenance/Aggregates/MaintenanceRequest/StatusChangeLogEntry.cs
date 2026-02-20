using K.Fixer.Domain.Shared;

namespace K.Fixer.Domain.Maintenance.Aggregates.MaintenanceRequest;

public sealed class StatusChangeLogEntry : Entity<Guid>
{
    public Guid     RequestId   { get; }
    public string   OldStatus   { get; }
    public string   NewStatus   { get; }
    public Guid     ChangedById { get; }
    public DateTime ChangedAt   { get; }
    
    internal long RecordId { get; private set; }
    
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    private StatusChangeLogEntry(){}
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.


    // Prywatny konstruktor — tworzony tylko przez agregat
    private StatusChangeLogEntry(
        Guid requestId,
        string oldStatus,
        string newStatus,
        Guid changedById)
        : base(Guid.NewGuid())
    {
        RequestId   = requestId;
        OldStatus   = oldStatus;
        NewStatus   = newStatus;
        ChangedById = changedById;
        ChangedAt   = DateTime.UtcNow;
    }

    // Factory method — jedyna droga tworzenia
    internal static StatusChangeLogEntry Create(
        Guid requestId,
        MaintenanceStatus from,
        MaintenanceStatus to,
        Guid changedById)
        => new(requestId, from.Value, to.Value, changedById);
}