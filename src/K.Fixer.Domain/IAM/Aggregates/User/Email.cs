using K.Fixer.Domain.Shared;

namespace K.Fixer.Domain.IAM.Aggregates.User;

public sealed class Email : ValueObject
{
    public string Value { get; }

    private Email(string value) => Value = value;

    public static Result<Email> Create(string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
            return Result.Failure<Email>("Email nie może być pusty.");

        var normalized = raw.Trim().ToLowerInvariant();

        if (normalized.Length > 250)
            return Result.Failure<Email>("Email nie może przekraczać 250 znaków.");

        // Prosta walidacja — w produkcji można użyć System.Net.Mail.MailAddress
        if (!System.Text.RegularExpressions.Regex.IsMatch(
                normalized, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
            return Result.Failure<Email>("Email ma nieprawidłowy format.");

        return Result.Success(new Email(normalized));
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Value;
}