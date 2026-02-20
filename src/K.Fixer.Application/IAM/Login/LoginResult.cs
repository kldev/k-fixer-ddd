namespace K.Fixer.Application.IAM.Login;

public sealed record LoginResult(
    Guid    UserId,
    string  FullName,
    string  Role,
    Guid?   CompanyId,
    string  Token
);