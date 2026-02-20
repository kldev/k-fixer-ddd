using K.Fixer.Domain.Shared;

namespace K.Fixer.Domain.PropertyManagement.Aggregates.Building;

public class CountryCode : ValueObject
{
    public static readonly CountryCode Poland      = new("PL");
    public static readonly CountryCode Germany     = new("DE");
    public static readonly CountryCode Belgium     = new("BE");
    
    public string Value { get; }
    private static readonly HashSet<string> ValidValues =
        ["PL", "DE", "BE"]; // Poland, Germany, Belgium 
    
    private CountryCode(string value) => Value = value;
    
    public static Result<CountryCode> Create(string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw) || !ValidValues.Contains(raw))
            return Result.Failure<CountryCode>($"Not allowed or wrong country core '{raw}'.");

        return Result.Success(new CountryCode(raw));
    }
    
    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Value;
}