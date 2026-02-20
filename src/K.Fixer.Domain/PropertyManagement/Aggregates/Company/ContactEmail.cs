using K.Fixer.Domain.Shared;

namespace K.Fixer.Domain.PropertyManagement.Aggregates.Company;

public sealed class ContactEmail : ValueObject
{
    public string Value { get; }

    private ContactEmail(string value) => Value = value;

    public static Result<ContactEmail> Create(string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
            return Result.Failure<ContactEmail>("The email can not be empty.");

        var normalized = raw.Trim().ToLowerInvariant();

        if (normalized.Length > 250)
            return Result.Failure<ContactEmail>("The email can not be longer than 250 characters.");

        if (!System.Text.RegularExpressions.Regex.IsMatch(
                normalized, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
            return Result.Failure<ContactEmail>("The email format is invalid.");

        return Result.Success(new ContactEmail(normalized));
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Value;
}