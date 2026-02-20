using K.Fixer.Domain.Shared;

namespace K.Fixer.Domain.IAM.Aggregates.User;

public sealed class User : AggregateRoot<Guid>
{
    public Email          Email          { get; private set; }
    public HashedPassword Password       { get; private set; }
    public FullName       FullName       { get; private set; }
    public UserRole       Role           { get; private set; }
    public Guid?          CompanyId      { get; private set; }
    public bool           IsActive       { get; private set; }
    public bool           IsLocked       { get; private set; }
    public DateTime       CreatedAt      { get; }
    
    public DateTime?       ModifiedAt     { get; private set; }     

    // EF Core wymaga bezparametrowego konstruktora — prywatny
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    private User() { }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

    private User(
        Guid id,
        Email email,
        HashedPassword password,
        FullName fullName,
        UserRole role,
        Guid? companyId)
        : base(id)
    {
        Email     = email;
        Password  = password;
        FullName  = fullName;
        Role      = role;
        CompanyId = companyId;
        IsActive  = true;
        IsLocked  = false;
        CreatedAt = DateTime.UtcNow;
    }

    // Factory method — jedyna publiczna droga tworzenia
    public static Result<User> Create(
        string email,
        string plainPassword,
        string fullName,
        string role,
        Guid? companyId = null)
    {
        var emailResult    = Email.Create(email);
        var passwordResult = HashedPassword.CreateFromPlainText(plainPassword);
        var nameResult     = FullName.Create(fullName);
        var roleResult     = UserRole.Create(role);

        // Collect all errors at once
        var errors = new List<string>();
        if (emailResult.IsFailure)    errors.Add(emailResult.Error!);
        if (passwordResult.IsFailure) errors.Add(passwordResult.Error!);
        if (nameResult.IsFailure)     errors.Add(nameResult.Error!);
        if (roleResult.IsFailure)     errors.Add(roleResult.Error!);

        if (errors.Count > 0)
            return Result.Failure<User>(string.Join("; ", errors));

        var user = new User(
            Guid.NewGuid(),
            emailResult.Value!,
            passwordResult.Value!,
            nameResult.Value!,
            roleResult.Value!,
            companyId);

        return Result.Success(user);
    }

    // Metody biznesowe — wszystkie zmiany przez metody, nie przez settery

    public Result VerifyPassword(string plainText)
        => Password.Verify(plainText)
            ? Result.Success()
            : Result.Failure("Wrong password.");

    public Result ChangePassword(string currentPlain, string newPlain)
    {
        var verifyResult = VerifyPassword(currentPlain);
        if (verifyResult.IsFailure) return verifyResult;

        var newPasswordResult = HashedPassword.CreateFromPlainText(newPlain);
        if (newPasswordResult.IsFailure) return Result.Failure(newPasswordResult.Error!);

        Password = newPasswordResult.Value!;
        ModifiedAt = DateTime.UtcNow;
        return Result.Success();
    }

    public Result Deactivate()
    {
        if (!IsActive) return Result.Failure("User is already deactivated.");
        IsActive = false;
        ModifiedAt = DateTime.UtcNow;
        return Result.Success();
    }

    public Result Lock()
    {
        if (IsLocked) return Result.Failure("User is already locked.");
        IsLocked = true;
        ModifiedAt = DateTime.UtcNow;
        return Result.Success();
    }

    public Result Unlock()
    {
        if (!IsLocked) return Result.Failure("User is already unlocked.");
        IsLocked = false;
        ModifiedAt = DateTime.UtcNow;
        return Result.Success();
    }

    public bool CanLogin() => IsActive && !IsLocked;
}