using K.Fixer.Domain.Shared;

namespace K.Fixer.Domain.PropertyManagement.Aggregates.Building;

public sealed class Address : ValueObject
{
    public string CountryCode { get; }
    public string Street  { get; }
    public string City    { get; }
    public string PostalCode { get; }

    private Address(string street, string city, string postalCode, string countryCode)
    {
        Street     = street;
        City       = city;
        PostalCode = postalCode;
        CountryCode = countryCode;
    }

    public static Result<Address> Create(string? street, string? city, string? postalCode, string? countryCode)
    {
        if (string.IsNullOrWhiteSpace(street))
            return Result.Failure<Address>("Street cannot be empty");

        if (string.IsNullOrWhiteSpace(city))
            return Result.Failure<Address>("City cannot be empty");

        if (string.IsNullOrWhiteSpace(postalCode))
            return Result.Failure<Address>("Postal code cannot be empty");
        
        if (string.IsNullOrWhiteSpace(countryCode))
            return Result.Failure<Address>("Country code invalid");

        if (IsPolandCountryCode(countryCode))
        {
            if (!System.Text.RegularExpressions.Regex.IsMatch(postalCode, @"^\d{2}-\d{3}$"))
                return Result.Failure<Address>("Postal code must be in format XX-XXX (np. 00-001).");
        }

        return Result.Success(new Address(street.Trim(), city.Trim(), postalCode, countryCode));
    }

    private static bool IsPolandCountryCode(string countryCode)
        => PropertyManagement.Aggregates.Building.CountryCode.Poland.Value == countryCode;
    
    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Street;
        yield return City;
        yield return PostalCode;
    }

    public override string ToString() => $"{Street}, {PostalCode} {City}";
}