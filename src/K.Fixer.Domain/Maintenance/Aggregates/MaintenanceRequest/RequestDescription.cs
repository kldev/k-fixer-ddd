using K.Fixer.Domain.Shared;

namespace K.Fixer.Domain.Maintenance.Aggregates.MaintenanceRequest;

public sealed class RequestDescription : ValueObject
{
    public string Value { get; }

    private RequestDescription(string value) => Value = value;

    public static Result<RequestDescription> Create(string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
            return Result.Failure<RequestDescription>("The request description can not be empty.");

        var trimmed = raw.Trim();

        if (trimmed.Length < 5)
            return Result.Failure<RequestDescription>("The request description must be at least 5 characters long.");

        if (trimmed.Length > 2000)
            return Result.Failure<RequestDescription>("The request description can not exceed 2000 characters long.");

        return Result.Success(new RequestDescription(trimmed));
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Value;
}