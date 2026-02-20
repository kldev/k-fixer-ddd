using K.Fixer.Domain.Shared;

namespace K.Fixer.Domain.Maintenance.Aggregates.MaintenanceRequest;

public sealed class WorkPeriod : ValueObject
{
    public DateTime StartedAt   { get; }
    public DateTime? CompletedAt { get; }

    private WorkPeriod(DateTime startedAt, DateTime? completedAt)
    {
        StartedAt   = startedAt;
        CompletedAt = completedAt;
    }

    // Creates a period when work begins
    public static WorkPeriod StartNow()
        => new(DateTime.UtcNow, null);

    // Closes the work period — creates a new VO
    public Result<WorkPeriod> Complete()
    {
        var completedAt = DateTime.UtcNow;

        if (completedAt <= StartedAt)
            return Result.Failure<WorkPeriod>(
                "The completion time cannot be earlier than the start time.");

        return Result.Success(new WorkPeriod(StartedAt, completedAt));
    }

    public TimeSpan? Duration =>
        CompletedAt.HasValue ? CompletedAt.Value - StartedAt : null;

    public bool IsCompleted => CompletedAt.HasValue;

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return StartedAt;
        yield return CompletedAt;
    }
}