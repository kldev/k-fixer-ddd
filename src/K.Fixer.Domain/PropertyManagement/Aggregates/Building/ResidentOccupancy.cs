using K.Fixer.Domain.Shared;

namespace K.Fixer.Domain.PropertyManagement.Aggregates.Building;

public sealed class ResidentOccupancy : Entity<Guid>
{
    public Guid           BuildingId  { get; }
    public Guid           ResidentId  { get; }
    public OccupancyPeriod Period     { get; private set; }
    
    internal long RecordId { get; private set; }
    
    #pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    private ResidentOccupancy(){}
    #pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
  
    private ResidentOccupancy(Guid buildingId, Guid residentId, OccupancyPeriod period)
        : base(Guid.NewGuid())
    {
        BuildingId = buildingId;
        ResidentId = residentId;
        Period     = period;
    }

    internal static Result<ResidentOccupancy> Create(
        Guid buildingId,
        Guid residentId,
        DateTime moveInDate)
    {
        var periodResult = OccupancyPeriod.Create(moveInDate, null);
        if (periodResult.IsFailure)
            return Result.Failure<ResidentOccupancy>(periodResult.Error!);

        return Result.Success(new ResidentOccupancy(buildingId, residentId, periodResult.Value!));
    }

    // Zameldowanie — zmiana periodu na zakończony
    internal Result EndOccupancy(DateTime moveOutDate)
    {
        var newPeriodResult = Period.EndOn(moveOutDate);
        if (newPeriodResult.IsFailure)
            return Result.Failure(newPeriodResult.Error!);

        Period = newPeriodResult.Value!;
        return Result.Success();
    }

    public bool IsActive() => Period.IsActive();
}