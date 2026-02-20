namespace K.Fixer.Application.IAM.Login;

public sealed record LoginCommand(
    string Email,
    string Password
);