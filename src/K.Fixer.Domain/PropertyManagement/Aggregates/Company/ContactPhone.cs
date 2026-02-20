using K.Fixer.Domain.Shared;

namespace K.Fixer.Domain.PropertyManagement.Aggregates.Company;

public sealed class ContactPhone : ValueObject
{
    public string Value { get; }  // znormalizowana forma: tylko cyfry + opcjonalny prefix +

    private ContactPhone(string value) => Value = value;

    public static Result<ContactPhone> Create(string? raw)
    {
        
        // Normalizacja: usuń spacje, myślniki, nawiasy
        var normalized = (raw ?? "").Trim()
            .Replace(" ", "")
            .Replace("-", "")
            .Replace("(", "")
            .Replace(")", "");
        // can be empty
        if (string.IsNullOrWhiteSpace(normalized))
            return Result.Success(new ContactPhone(normalized));

        // Format: opcjonalny +, potem 7–15 cyfr
        if (!System.Text.RegularExpressions.Regex.IsMatch(normalized, @"^\+?[0-9]{7,15}$"))
            return Result.Failure<ContactPhone>(
                "The telephone number must contain between 7 and 15 digits (optionally with a + prefix).");

        return Result.Success(new ContactPhone(normalized));
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Value;
}