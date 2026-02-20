using K.Fixer.Domain.Shared;

namespace K.Fixer.Domain.PropertyManagement.Aggregates.Building;

public sealed class OccupancyPeriod : ValueObject
{
    public DateTime From { get; }
    public DateTime? To  { get; }

    private OccupancyPeriod(DateTime from, DateTime? to)
    {
        From = from;
        To   = to;
    }

    public static Result<OccupancyPeriod> Create(DateTime from, DateTime? to)
    {
        if (to.HasValue && to.Value <= from)
            return Result.Failure<OccupancyPeriod>(
                "The to dates must be greater than the from date for this period");

        return Result.Success(new OccupancyPeriod(from, to));
    }

    // Pobyt jest aktywny gdy brak daty zakończenia
    public bool IsActive() => !To.HasValue;

    // Pobyt był aktywny w danym dniu
    public bool WasActiveOn(DateTime date)
        => date >= From && (!To.HasValue || date <= To.Value);

    // Zamknięcie pobytu — tworzy nowy VO (immutability)
    public Result<OccupancyPeriod> EndOn(DateTime endDate)
        => Create(From, endDate);

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return From;
        yield return To;
    }
}