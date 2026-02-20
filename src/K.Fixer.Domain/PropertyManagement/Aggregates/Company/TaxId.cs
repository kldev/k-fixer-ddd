using K.Fixer.Domain.Shared;

namespace K.Fixer.Domain.PropertyManagement.Aggregates.Company;

public sealed class TaxId : ValueObject
{
    public string CountryCode { get; }  // e.g. "PL", "DE", "US"
    public string Number      { get; }  // number only, without country code and dashes

    // Full value for storing in the database and comparison: "PL1234567890"
    public string Value => $"{CountryCode}{Number}";

    private TaxId(string countryCode, string number)
    {
        CountryCode = countryCode;
        Number      = number;
    }

    public static Result<TaxId> Create(string? countryCode, string? number)
    {
        if (string.IsNullOrWhiteSpace(countryCode))
            return Result.Failure<TaxId>("Country code can not be empty");

        var country = countryCode.Trim().ToUpperInvariant();

        if (country.Length != 2 || !country.All(char.IsLetter))
            return Result.Failure<TaxId>("Country code must be a 2-letter ISO code (e.g. PL, DE, US).");

        if (string.IsNullOrWhiteSpace(number))
            return Result.Failure<TaxId>("Tax identification number cannot be empty.");

        // Normalization — remove dashes, spaces, slashes
        var normalized = number.Trim()
            .Replace("-", "")
            .Replace(" ", "")
            .Replace("/", "")
            .ToUpperInvariant();

        if (normalized.Length < 5 || normalized.Length > 20)
            return Result.Failure<TaxId>(
                "Tax identification number must be between 5 and 20 characters.");

        // Country-specific validation — easy to extend with new countries
        var countryValidation = country switch
        {
            "PL" => ValidatePolishNip(normalized),
            "DE" => ValidateGermanTaxId(normalized),
            "GB" => ValidateUkVatNumber(normalized),
            _    => ValidateGeneric(normalized),   // fallback for other countries
        };

        if (countryValidation.IsFailure)
            return Result.Failure<TaxId>(countryValidation.Error!);

        return Result.Success(new TaxId(country, normalized));
    }

    // ─── Per-country validators ───────────────────────────────────────────────

    // Poland: NIP — 10 digits, checksum algorithm
    private static Result ValidatePolishNip(string number)
    {
        if (number.Length != 10 || !number.All(char.IsDigit))
            return Result.Failure("Polish NIP must consist of exactly 10 digits.");

        int[] weights = [6, 5, 7, 2, 3, 4, 5, 6, 7];
        var sum = weights.Select((w, i) => w * (number[i] - '0')).Sum();
        if (sum % 11 != (number[9] - '0'))
            return Result.Failure("Polish NIP has an invalid checksum.");

        return Result.Success();
    }

    // Germany: Steuernummer — 10-11 digits
    private static Result ValidateGermanTaxId(string number)
    {
        if ((number.Length != 10 && number.Length != 11) || !number.All(char.IsDigit))
            return Result.Failure("German tax number must be 10 or 11 digits.");

        return Result.Success();
    }

    // United Kingdom: VAT Number — "GB" + 9 digits or other format
    private static Result ValidateUkVatNumber(string number)
    {
        // Format: 9 digits or "GD"/"HA" + 3 digits
        if (number.Length == 9 && number.All(char.IsDigit))
            return Result.Success();

        if (number.Length == 5 &&
            (number.StartsWith("GD") || number.StartsWith("HA")) &&
            number[2..].All(char.IsDigit))
            return Result.Success();

        return Result.Failure("UK VAT number must be 9 digits or GD/HA + 3 digits.");
    }

    // Fallback — basic validation for countries without a dedicated validator
    private static Result ValidateGeneric(string number)
    {
        if (!number.All(c => char.IsLetterOrDigit(c)))
            return Result.Failure(
                "Tax identification number may contain only letters and digits.");

        return Result.Success();
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return CountryCode;
        yield return Number;
    }

    // Country-specific display formatting
    public string Formatted => CountryCode switch
    {
        "PL" => $"{Number[..3]}-{Number[3..6]}-{Number[6..8]}-{Number[8..]}",
        "DE" => $"{Number[..3]}/{Number[3..]}",
        _    => $"{CountryCode} {Number}",
    };

    public override string ToString() => Value;
}