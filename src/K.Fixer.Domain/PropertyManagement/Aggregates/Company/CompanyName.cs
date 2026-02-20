using K.Fixer.Domain.Shared;

namespace K.Fixer.Domain.PropertyManagement.Aggregates.Company;

public sealed class CompanyName : ValueObject
{
    public string Value { get; }

    private CompanyName(string value) => Value = value;

    public static Result<CompanyName> Create(string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
            return Result.Failure<CompanyName>("The company name is invalid.");

        var trimmed = raw.Trim();

        if (trimmed.Length < 2)
            return Result.Failure<CompanyName>("The company name must be at least 2 characters long.");

        if (trimmed.Length > 200)
            return Result.Failure<CompanyName>("The company name can not be longer than 200 characters.");

        return Result.Success(new CompanyName(trimmed));
    }

    // Dwie firmy o tej samej nazwie (różna wielkość liter) to ta sama firma
    // Equals i GetHashCode działają case-insensitive
    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value.ToUpperInvariant();
    }

    public override string ToString() => Value;
}