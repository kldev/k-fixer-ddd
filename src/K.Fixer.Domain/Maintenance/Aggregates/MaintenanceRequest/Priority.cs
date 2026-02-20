using K.Fixer.Domain.Shared;

namespace K.Fixer.Domain.Maintenance.Aggregates.MaintenanceRequest;

public sealed class Priority : ValueObject
{
    public static readonly Priority Low    = new("Low", 1);
    public static readonly Priority Medium = new("Medium", 2);
    public static readonly Priority High   = new("High", 3);
    public static readonly Priority Urgent = new("Urgent", 4);

    public string Value    { get; }
    public int    NumericValue { get; }

    private static readonly Dictionary<string, Priority> _all = new()
    {
        ["Low"]    = Low,
        ["Medium"] = Medium,
        ["High"]   = High,
        ["Urgent"] = Urgent,
    };

    private Priority(string value, int numericValue)
    {
        Value        = value;
        NumericValue = numericValue;
    }

    public static Result<Priority> Create(string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw) || !_all.TryGetValue(raw, out var priority))
            return Result.Failure<Priority>($"Priorytet '{raw}' nie istnieje.");

        return Result.Success(priority);
    }

    // Porównanie priorytetów — Urgent > High > Medium > Low
    public bool IsHigherThan(Priority other) => NumericValue > other.NumericValue;

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Value;
}