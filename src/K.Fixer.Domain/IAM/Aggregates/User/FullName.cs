using K.Fixer.Domain.Shared;

namespace K.Fixer.Domain.IAM.Aggregates.User;

public sealed class FullName : ValueObject
{
    public string Value { get; }

    private FullName(string value) => Value = value;

    public static Result<FullName> Create(string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
            return Result.Failure<FullName>("Imię i nazwisko nie może być puste.");

        var trimmed = raw.Trim();

        if (trimmed.Length < 2)
            return Result.Failure<FullName>("Imię i nazwisko musi mieć co najmniej 2 znaki.");

        if (trimmed.Length > 100)
            return Result.Failure<FullName>("Imię i nazwisko nie może przekraczać 100 znaków.");

        return Result.Success(new FullName(trimmed));
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Value;
}