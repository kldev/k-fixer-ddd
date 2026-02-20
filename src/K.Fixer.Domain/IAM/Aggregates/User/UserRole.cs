using K.Fixer.Domain.Shared;

namespace K.Fixer.Domain.IAM.Aggregates.User;

// "Smart enum" — nie zwykły enum, bo zawiera logikę domenową
public sealed class UserRole : ValueObject
{
    // Statyczne instancje — dostępne jak enum, ale z metodami
    public static readonly UserRole Admin          = new("Admin", 1);
    public static readonly UserRole CompanyAdmin   = new("CompanyAdmin", 2);
    public static readonly UserRole Technician     = new("Technician", 3);
    public static readonly UserRole Resident       = new("Resident", 4);

    public string Value { get; }
    public int NumericValue { get; }

    private static readonly HashSet<string> ValidValues =
        ["Admin", "CompanyAdmin", "Technician", "Resident"];
    
    private static readonly Dictionary<string, UserRole> _all = new()
    {
        ["Admin"]    = Admin,
        ["CompanyAdmin"] = CompanyAdmin,
        ["Technician"]   = Technician,
        ["Resident"] = Resident,
    };

    private UserRole(string value, int numericValue)
    {
        Value = value;
        NumericValue = numericValue;
    }

    public static Result<UserRole> Create(string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw) || !ValidValues.Contains(raw))
            return Result.Failure<UserRole>($"Rola '{raw}' nie istnieje.");

        _all.TryGetValue(raw, out var role);
        
        return Result.Success(role!);
    }

    // Logika domenowa zawarta w Value Object
    public bool CanCreateMaintenanceRequest()  => this == Resident;
    public bool CanAssignTechnicians()         => this == CompanyAdmin || this == Admin;
    public bool CanStartWork()                 => this == Technician;
    public bool CanCloseRequest()              => this == CompanyAdmin || this == Admin;
    public bool CanManageCompanies()           => this == Admin;

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Value;
}