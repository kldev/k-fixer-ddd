using K.Fixer.Domain.Shared;

namespace K.Fixer.Domain.Maintenance.Aggregates.MaintenanceRequest;

public sealed class ResolutionNote : ValueObject
{
    public string Value { get; }

    private ResolutionNote(string value) => Value = value;

    public static Result<ResolutionNote> Create(string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
            return Result.Failure<ResolutionNote>(
                "The resolution note is required after completion.");

        var trimmed = raw.Trim();

        if (trimmed.Length < 10)
            return Result.Failure<ResolutionNote>(
                "The resolution note must be at least 10 characters — describe what was done.");

        if (trimmed.Length > 2000)
            return Result.Failure<ResolutionNote>("The resolution note cannot exceed 2000 characters.");

        return Result.Success(new ResolutionNote(trimmed));
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }
}