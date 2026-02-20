using K.Fixer.Domain.Shared;

namespace K.Fixer.Domain.Maintenance.Aggregates.MaintenanceRequest;

public sealed class MaintenanceStatus : ValueObject
{
    public static readonly MaintenanceStatus New       = new("New", 1);
    public static readonly MaintenanceStatus Assigned  = new("Assigned", 2);
    public static readonly MaintenanceStatus InProgress = new("InProgress", 3);
    public static readonly MaintenanceStatus Completed = new("Completed", 20);
    public static readonly MaintenanceStatus Closed    = new("Closed", 21);
    public static readonly MaintenanceStatus Reopened  = new("Reopened", 22);
    public static readonly MaintenanceStatus Cancelled = new("Cancelled", 23);

    public string Value { get; }
    public int    NumericValue { get; }
    
    private static readonly Dictionary<string, MaintenanceStatus> _all = new()
    {
        ["New"]    = New,
        ["Assigned"] = Assigned,
        ["InProgress"]   = InProgress,
        ["Completed"] = Completed,
        ["Closed"] = Closed,
        ["Reopened"] = Reopened,
        ["Cancelled"] = Cancelled,
    };

    private static readonly Dictionary<string, HashSet<string>> _allowedTransitions = new()
    {
        ["New"]        = ["Assigned", "Cancelled"],
        ["Assigned"]   = ["InProgress", "Cancelled"],
        ["InProgress"] = ["Completed", "Cancelled"],
        ["Completed"]  = ["Closed", "Reopened", "Cancelled"],
        ["Reopened"]   = ["InProgress", "Cancelled"],
        ["Closed"]     = [],   // stan końcowy
        ["Cancelled"]  = [],   // stan końcowy
    };

    private MaintenanceStatus(string value, int numericValue)
    {
        Value = value;
        NumericValue = numericValue;
    }

    public static Result<MaintenanceStatus> Create(string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw) || !_allowedTransitions.ContainsKey(raw))
            return Result.Failure<MaintenanceStatus>($"Status '{raw}' not recognized.");

        _all.TryGetValue(raw, out var status);

        return Result.Success(status!);
    }

    // Sprawdza czy przejście do nowego statusu jest dozwolone
    public bool CanTransitionTo(MaintenanceStatus next)
        => _allowedTransitions.TryGetValue(Value, out var allowed)
           && allowed.Contains(next.Value);

    // Wygodne metody sprawdzające stan
    public bool IsTerminal() => this == Closed || this == Cancelled;
    public bool IsEditable() => this == New;
    public bool AllowsPriorityChange()
        => this != Closed && this != Cancelled;

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Value;
}