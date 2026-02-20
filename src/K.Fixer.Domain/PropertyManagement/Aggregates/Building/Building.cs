using K.Fixer.Domain.Shared;

namespace K.Fixer.Domain.PropertyManagement.Aggregates.Building;

public sealed class Building : AggregateRoot<Guid>
{
    public BuildingName Name     { get; private set; }
    public Address      Address  { get; private set; }
    public CountryCode  CountryCode { get; private set; }
    public Guid         CompanyId { get; }
    public bool         IsActive  { get; private set; }
    public DateTime     CreatedAt { get; }
    
    private readonly List<ResidentOccupancy> _occupancies = [];
    public IReadOnlyCollection<ResidentOccupancy> Occupancies
        => _occupancies.AsReadOnly();

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    private Building() { }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

    private Building(Guid id, Guid companyId, BuildingName name, Address address, CountryCode countryCode)
        : base(id)
    {
        CompanyId = companyId;
        Name      = name;
        Address   = address;
        CountryCode = countryCode;
        IsActive  = true;
        CreatedAt = DateTime.UtcNow;
    }

    public static Result<Building> Create(
        Guid companyId,
        string name,
        string street,
        string city,
        string postalCode,
        string countryCode,
        Guid? fixedGuid = null)
    {
        var nameResult = BuildingName.Create(name);
        var addressResult = Address.Create(street, city, postalCode, countryCode);
        var countryResult = CountryCode.Create(countryCode);

        if (nameResult.IsFailure) return Result.Failure<Building>(nameResult.Error!);
        if (addressResult.IsFailure) return Result.Failure<Building>(addressResult.Error!);
        if (countryResult.IsFailure) return Result.Failure<Building>(countryResult.Error!);

        return Result.Success(new Building(
            fixedGuid ?? Guid.NewGuid(), companyId, nameResult.Value!, addressResult.Value!, countryResult.Value!));
    }

    // Assign a resident to the building
    public Result AssignResident(Guid residentId, DateTime moveInDate)
    {
        if (!IsActive)
            return Result.Failure("Can not assign resident because to inactive building.");

        // Check if the resident already has an active occupancy
        var hasActive = _occupancies.Any(o => o.ResidentId == residentId && o.IsActive());
        if (hasActive)
            return Result.Failure("The resident already has an active assignment in this building.");

        var occupancyResult = ResidentOccupancy.Create(Id, residentId, moveInDate);
        if (occupancyResult.IsFailure)
            return Result.Failure(occupancyResult.Error!);

        _occupancies.Add(occupancyResult.Value!);
        return Result.Success();
    }

    // Remove a resident (check-out)
    public Result RemoveResident(Guid residentId, DateTime moveOutDate)
    {
        var occupancy = _occupancies
            .FirstOrDefault(o => o.ResidentId == residentId && o.IsActive());

        if (occupancy is null)
            return Result.Failure("The resident doesn't have active assigment in this building.");

        return occupancy.EndOccupancy(moveOutDate);
    }

    // Check if the resident currently lives in the building
    public bool HasActiveResident(Guid residentId)
        => _occupancies.Any(o => o.ResidentId == residentId && o.IsActive());

    public Result Update(string name, string street, string city, string postalCode, string countryCode)
    {
        var nameResult    = BuildingName.Create(name);
        var addressResult = Address.Create(street, city, postalCode, countryCode);
        var countryResult = CountryCode.Create(countryCode);

        if (nameResult.IsFailure)    return Result.Failure(nameResult.Error!);
        if (addressResult.IsFailure) return Result.Failure(addressResult.Error!);
        if (countryResult.IsFailure) return Result.Failure(countryResult.Error!);

        Name        = nameResult.Value!;
        Address     = addressResult.Value!;
        CountryCode = countryResult.Value!;
        return Result.Success();
    }

    public Result Deactivate()
    {
        if (!IsActive) return Result.Failure("The building is already inactive");
        IsActive = false;
        return Result.Success();
    }
}