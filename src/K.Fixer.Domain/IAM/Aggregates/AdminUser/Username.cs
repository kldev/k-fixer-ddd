using K.Fixer.Domain.Shared;

namespace K.Fixer.Domain.IAM.Aggregates.AdminUser;

public sealed class Username : ValueObject
{
    public string Value { get; }
    
    // ReSharper disable once ConvertToPrimaryConstructor
    public Username(string value) => Value = value;

    public static Result<Username> Create(string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
            return Result.Failure<Username>("Username can not be empty.");
        
        var trimmed = raw.Trim();

        if (trimmed.Length < 2)
            return Result.Failure<Username>("Username must be at least 2 characters long.");

        if (trimmed.Length > 30)
            return Result.Failure<Username>("Username can not exceeds 30 characters long.");

        return Result.Success(new Username(trimmed));
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }
    
    public override string ToString() => Value;
}