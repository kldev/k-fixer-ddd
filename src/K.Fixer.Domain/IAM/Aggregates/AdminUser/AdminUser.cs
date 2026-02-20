using K.Fixer.Domain.IAM.Aggregates.User;
using K.Fixer.Domain.Shared;

namespace K.Fixer.Domain.IAM.Aggregates.AdminUser;

public sealed class AdminUser : AggregateRoot<Guid>
{
    public Username Username { get; private set; }
    public HashedPassword Password { get; private set; }
    public UserRole Role { get; private set; }
    public FullName FullName { get; private set; }
    public DateTime CreatedAt { get; private set; }

    // EF
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    private AdminUser() { }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

    private AdminUser(Guid id,
        Username username,
        FullName fullName,
        HashedPassword password
    ) : base(id)
    {
        Username = username;
        FullName = fullName;
        Password = password;
        Role = UserRole.Admin;
        CreatedAt = DateTime.UtcNow;
    }

    public static Result<AdminUser> Create(
        string username,
        string plainPassword,
        string fullName)
    {
        var usernameResult = Username.Create(username);
        var passwordResult = HashedPassword.CreateFromPlainText(plainPassword);
        var nameResult = FullName.Create(fullName);
        
        var errors = new List<string>();
        if (usernameResult.IsFailure) errors.Add(usernameResult.Error!);
        if (passwordResult.IsFailure) errors.Add(passwordResult.Error!);
        if (nameResult.IsFailure) errors.Add(nameResult.Error!);

        if (errors.Count > 0)
            return Result.Failure<AdminUser>(string.Join("; ", errors));

        var user = new AdminUser(
            Guid.NewGuid(),
            usernameResult.Value!,
            nameResult.Value!,
            passwordResult.Value!
        );

        return Result.Success(user);
    }
    
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
        return Result.Success();
    }

}