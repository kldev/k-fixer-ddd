using K.Fixer.Domain.Shared;

namespace K.Fixer.Domain.PropertyManagement.Aggregates.Building;

public sealed class BuildingName : ValueObject
{
    public string Value { get; }
    
    // ReSharper disable once ConvertToPrimaryConstructor
    public BuildingName(string value) => Value = value;

    public static Result<BuildingName> Create(string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
            return Result.Failure<BuildingName>("The building name can not be empty.");
        
        var trimmed = raw.Trim();

        if (trimmed.Length < 2)
            return Result.Failure<BuildingName>("The building name can not be less than 2 characters.");

        if (trimmed.Length > 150)
            return Result.Failure<BuildingName>("The building name can not be more than 150 characters.");

        return Result.Success(new BuildingName(trimmed));
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }
    
    public override string ToString() => Value;
}