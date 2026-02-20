using K.Fixer.Domain.Shared;

namespace K.Fixer.Domain.Maintenance.Aggregates.MaintenanceRequest;

public sealed class RequestTitle : ValueObject
{
    public string Value { get; }

    private RequestTitle(string value) => Value = value;

    public static Result<RequestTitle> Create(string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
            return Result.Failure<RequestTitle>("The request title can not be empty.");

        var trimmed = raw.Trim();

        if (trimmed.Length < 5)
            return Result.Failure<RequestTitle>("The request title must be at least 5 characters long.");

        if (trimmed.Length > 200)
            return Result.Failure<RequestTitle>("The request title can not exceed 200 characters long.");

        return Result.Success(new RequestTitle(trimmed));
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Value;
}