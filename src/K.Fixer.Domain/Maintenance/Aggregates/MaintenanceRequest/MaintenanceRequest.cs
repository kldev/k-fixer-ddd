using K.Fixer.Domain.Maintenance.Events;
using K.Fixer.Domain.Shared;

namespace K.Fixer.Domain.Maintenance.Aggregates.MaintenanceRequest;

public sealed class MaintenanceRequest : AggregateRoot<Guid>
{
    public RequestTitle       Title          { get; private set; }
    public RequestDescription Description    { get; private set; }
    public MaintenanceStatus  Status         { get; private set; }
    public Priority           Priority       { get; private set; }
    public Guid               BuildingId     { get; }
    public Guid               CreatedById    { get; }
    public Guid?              AssignedToId   { get; private set; }
    public ResolutionNote?    ResolutionNote { get; private set; }
    public WorkPeriod?        WorkPeriod     { get; private set; }
    public DateTime           CreatedAt      { get; }
    public DateTime?          ModifiedAt     { get; private set; }
    public bool               IsDeleted      { get; private set; }

    private readonly List<StatusChangeLogEntry> _statusLog = [];
    public IReadOnlyCollection<StatusChangeLogEntry> StatusLog
        => _statusLog.AsReadOnly();

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    private MaintenanceRequest() { }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

    private MaintenanceRequest(
        Guid id,
        Guid buildingId,
        Guid createdById,
        RequestTitle title,
        RequestDescription description)
        : base(id)
    {
        BuildingId  = buildingId;
        CreatedById = createdById;
        Title       = title;
        Description = description;
        Status      = MaintenanceStatus.New;
        Priority    = Priority.Low;
        CreatedAt   = DateTime.UtcNow;
    }

    // ─── Factory ────────────────────────────────────────────────────────────

    public static Result<MaintenanceRequest> Create(
        Guid buildingId,
        Guid residentId,
        string title,
        string description)
    {
        var titleResult = RequestTitle.Create(title);
        var descResult  = RequestDescription.Create(description);

        if (titleResult.IsFailure) return Result.Failure<MaintenanceRequest>(titleResult.Error!);
        if (descResult.IsFailure)  return Result.Failure<MaintenanceRequest>(descResult.Error!);
        
        
        var request = new MaintenanceRequest(
            Guid.NewGuid(), buildingId, residentId,
            titleResult.Value!, descResult.Value!);

        // Aggregate emits event upon creation
        request.RaiseDomainEvent(new MaintenanceRequestCreatedEvent(
            request.Id, buildingId, residentId, title, DateTime.UtcNow));

        return Result.Success(request);
    }

    // ─── Edit ───────────────────────────────────────────────────────────────

    public Result Edit(Guid editorId, string newTitle, string newDescription)
    {
        if (!Status.IsEditable())
            return Result.Failure("A request can only be edited when in New status.");

        if (editorId != CreatedById)
            return Result.Failure("Only the creator of the request can edit it.");

        var titleResult = RequestTitle.Create(newTitle);
        var descResult  = RequestDescription.Create(newDescription);

        if (titleResult.IsFailure) return Result.Failure(titleResult.Error!);
        if (descResult.IsFailure)  return Result.Failure(descResult.Error!);

        Title       = titleResult.Value!;
        Description = descResult.Value!;
        ModifiedAt  = DateTime.UtcNow;

        return Result.Success();
    }

    // ─── Assign technician ──────────────────────────────────────────────────

    public Result AssignTo(Guid technicianId, Guid assignedByAdminId)
    {
        if (!Status.CanTransitionTo(MaintenanceStatus.Assigned))
            return Result.Failure(
                $"Cannot assign a technician to a request in status '{Status}'.");

        var logEntry = StatusChangeLogEntry.Create(
            Id, Status, MaintenanceStatus.Assigned, assignedByAdminId);

        AssignedToId = technicianId;
        Status       = MaintenanceStatus.Assigned;
        ModifiedAt   = DateTime.UtcNow;
        _statusLog.Add(logEntry);

        RaiseDomainEvent(new TechnicianAssignedEvent(
            Id, technicianId, assignedByAdminId, DateTime.UtcNow));

        return Result.Success();
    }

    // ─── Start work ─────────────────────────────────────────────────────────

    public Result StartWork(Guid technicianId)
    {
        if (!Status.CanTransitionTo(MaintenanceStatus.InProgress))
            return Result.Failure(
                $"Cannot start work on a request in status '{Status}'.");

        if (AssignedToId != technicianId)
            return Result.Failure("Only the assigned technician can start work.");

        var logEntry = StatusChangeLogEntry.Create(
            Id, Status, MaintenanceStatus.InProgress, technicianId);

        Status     = MaintenanceStatus.InProgress;
        WorkPeriod = WorkPeriod.StartNow();
        ModifiedAt = DateTime.UtcNow;
        _statusLog.Add(logEntry);

        RaiseDomainEvent(new WorkStartedEvent(Id, technicianId, DateTime.UtcNow));

        return Result.Success();
    }

    // ─── Complete work ───────────────────────────────────────────────────────

    public Result Complete(Guid technicianId, string resolutionNoteText)
    {
        if (!Status.CanTransitionTo(MaintenanceStatus.Completed))
            return Result.Failure(
                $"Cannot complete a request in status '{Status}'.");

        if (AssignedToId != technicianId)
            return Result.Failure("Only the assigned technician can complete the request.");

        var noteResult = ResolutionNote.Create(resolutionNoteText);
        if (noteResult.IsFailure) return Result.Failure(noteResult.Error!);

        var completedPeriod = WorkPeriod!.Complete();
        if (completedPeriod.IsFailure) return Result.Failure(completedPeriod.Error!);

        var logEntry = StatusChangeLogEntry.Create(
            Id, Status, MaintenanceStatus.Completed, technicianId);

        Status         = MaintenanceStatus.Completed;
        ResolutionNote = noteResult.Value!;
        WorkPeriod     = completedPeriod.Value!;
        ModifiedAt     = DateTime.UtcNow;
        _statusLog.Add(logEntry);

        RaiseDomainEvent(new WorkCompletedEvent(
            Id, technicianId, resolutionNoteText, DateTime.UtcNow));

        return Result.Success();
    }

    // ─── Close request ───────────────────────────────────────────────────────

    public Result Close(Guid adminId)
    {
        if (!Status.CanTransitionTo(MaintenanceStatus.Closed))
            return Result.Failure(
                $"Cannot close a request in status '{Status}'.");

        var logEntry = StatusChangeLogEntry.Create(
            Id, Status, MaintenanceStatus.Closed, adminId);

        Status     = MaintenanceStatus.Closed;
        ModifiedAt = DateTime.UtcNow;
        _statusLog.Add(logEntry);

        RaiseDomainEvent(new RequestClosedEvent(Id, adminId, DateTime.UtcNow));

        return Result.Success();
    }

    // ─── Reopen ─────────────────────────────────────────────────────────────

    public Result Reopen(Guid adminId, string reason)
    {
        if (!Status.CanTransitionTo(MaintenanceStatus.Reopened))
            return Result.Failure(
                $"Cannot reopen a request in status '{Status}'.");

        var logEntry = StatusChangeLogEntry.Create(
            Id, Status, MaintenanceStatus.Reopened, adminId);

        Status     = MaintenanceStatus.Reopened;
        ModifiedAt = DateTime.UtcNow;
        _statusLog.Add(logEntry);

        RaiseDomainEvent(new RequestReopenedEvent(Id, adminId, reason, DateTime.UtcNow));

        return Result.Success();
    }

    // ─── Cancel ──────────────────────────────────────────────────────────────

    public Result Cancel(Guid cancelledById, string cancellerRole)
    {
        if (!Status.CanTransitionTo(MaintenanceStatus.Cancelled))
            return Result.Failure(
                $"Cannot cancel a request in status '{Status}'.");

        var logEntry = StatusChangeLogEntry.Create(
            Id, Status, MaintenanceStatus.Cancelled, cancelledById);

        Status     = MaintenanceStatus.Cancelled;
        ModifiedAt = DateTime.UtcNow;
        _statusLog.Add(logEntry);

        RaiseDomainEvent(new RequestCancelledEvent(
            Id, cancelledById, cancellerRole, DateTime.UtcNow));

        return Result.Success();
    }

    // ─── Change priority ─────────────────────────────────────────────────────

    public Result ChangePriority(string newPriorityValue, Guid changedById)
    {
        if (!Status.AllowsPriorityChange())
            return Result.Failure(
                $"Cannot change priority of a request in status '{Status}'.");

        var priorityResult = Priority.Create(newPriorityValue);
        if (priorityResult.IsFailure) return Result.Failure(priorityResult.Error!);

        Priority   = priorityResult.Value!;
        ModifiedAt = DateTime.UtcNow;

        return Result.Success();
    }

    // Soft delete — never permanently removed
    public void SoftDelete()
    {
        IsDeleted  = true;
        ModifiedAt = DateTime.UtcNow;
    }
}